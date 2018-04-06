/**
 * @description Timer services class implementing the api logic
 * @copyright (c) 2017 Honest Insurance
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using System;
using System.Net;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using ServiceStack;
using ApiService.ServiceModel;
using Nethereum;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;


namespace ApiService.ServiceInterface
{
    public class TimerServices : Service
    {
        public object Get(GetTimerNotifications request) {
            // Maximal duration that can be specified to retrive notifications for (24 hours)
            ulong maxDuration = 3600 * 24;
            // Get the contract for the Bond by specifying the bond address
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.TIMER.abi, AppServices.GetEcosystemAdr(request.ContractAdr).TimerContractAdr);

            // Get the current time from the blockchain from the timer contract
            ulong currentTimeEPOCH = contract.GetFunction("getBlockchainEPOCHTime").CallAsync<ulong>().Result;

            // Get the inception date of the timer contract
            ulong timerInceptionEPOCH = contract.GetFunction("TIMER_INCEPTION_DATE").CallAsync<ulong>().Result;

            // If both from and to time parameter are provided ensure that they are at least the value of the timer inception EPOCH
            if (((request.FromTime != 0) && (request.FromTime < timerInceptionEPOCH)) || ((request.ToTime != 0) && (request.ToTime < timerInceptionEPOCH)))
                throw new HttpError(HttpStatusCode.NotAcceptable, AppModelConfig.TransactionProcessingError, AppModelConfig.TransactionProcessingErrorMessage);
            
            // If no time parameter is provided
            if ((request.FromTime == 0 && request.ToTime == 0)) {
                request.FromTime = currentTimeEPOCH;
                request.ToTime = currentTimeEPOCH + maxDuration;
            }
            // If only the From time is not provided set it based on the to time provided
            else if (request.FromTime == 0) 
                request.FromTime = request.ToTime - maxDuration;
            // If only the To time is not provided set it based on the from time provided
            else if (request.ToTime == 0) 
                request.ToTime = request.FromTime + maxDuration;
            // If both from and to time is provided ensure they are valid (to is greater than from) and within max duration allowed 
            else if ((request.FromTime + maxDuration < request.ToTime) || (request.FromTime > request.ToTime))
                throw new HttpError(HttpStatusCode.NotAcceptable, AppModelConfig.TransactionProcessingError, AppModelConfig.TransactionProcessingErrorMessage);
            
            TimerNotifications notifications = new TimerNotifications {
                NotificationEntries = new List<TimerNotificationEntry>()
            };

            ulong fromTime_10_S = request.FromTime / 10;
            ulong toTime_10_S = request.ToTime / 10;

            // 100 second loop interation
            for (ulong i_100_S = fromTime_10_S / 10; i_100_S <= toTime_10_S / 10; i_100_S++) {
                // Verify it this interval has entries
                if (contract.GetFunction("timeIntervalHasEntries").CallAsync<bool>(i_100_S).Result == true) {
                    // Iterate throught the 10 slots within this 100 second itervall
                    for (ulong j_10_S = i_100_S * 10; j_10_S < (i_100_S + 1) * 10; j_10_S++) {
                        // Get the number of entries for this 10 second slot
                        ulong countEntries = contract.GetFunction("getTimerNotificationCount").CallAsync<ulong>(j_10_S).Result;
                        // Iterate through every entry in a slot
                        for (uint k=0; k<countEntries; k++) {
                            // Verify if the entry is within from and to before adding it to the list
                            if ((j_10_S >= fromTime_10_S) && (j_10_S <= toTime_10_S)) {

                                var entry = contract.GetFunction("notification").CallDeserializingToObjectAsync<TimerNotificationEntry>(j_10_S, k).Result;
                                entry.Timestamp = j_10_S * 10;
                                notifications.NotificationEntries.Add(entry);
                            }
                        }
                    }
                }
            }
            // Return all the notifications
            return notifications;
        }

        public object Put(PingTimer request) {
            string timerContractAdr = AppServices.GetEcosystemAdr(request.ContractAdr).TimerContractAdr;
            // Activate or deactivate the auto ping scheduling functionality
            AppServices.configureTimerPing(timerContractAdr, request.SigningPrivateKey, request.AutoSchedulePingDuration);
            // Submit and return the transaction hash of the broadcasted ping transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.TIMER.abi, 
                timerContractAdr,
                request.SigningPrivateKey,
                "ping"
            );
        }
    }
}