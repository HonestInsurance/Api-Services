/**
 * @description Adjustor services class implementing the api logic
 * @copyright (c) 2017 HIC Limited (NZBN: 9429043400973)
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
    public class AdjustorServices : Service
    {
        public object Get(GetAdjustorList request) {
            // Get the contract for the Adjustor by specifying the adjustor address
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.BOND,  AppServices.GetEcosystemAdr(request.ContractAdr).AdjustorContractAdr);

            // Create the adjustor list object and initialise
            AdjustorList list = new AdjustorList() { Items = new List<AdjustorDetail>() };

            // Get the metadata info for the first, next and count of Adjustors
            list.Info = contract.GetFunction("hashMap").CallDeserializingToObjectAsync<ListInfo>().Result;
            // Create the idx to continue the search from
            int lastIdx = (request.FromIdx != 0 ? (int)request.FromIdx :(int)list.Info.CountAllItems);
            // Define the lower bound 
            int lowerBoundIdx = Math.Max(0, lastIdx - 
                (request.MaxEntries != 0 ? (int)request.MaxEntries : (int)AppModelConfig.defaultNumberEntriesForLazyLoading));
            // Iterate through all the possilbe adjustor entries
            for (int i=lastIdx; i>lowerBoundIdx; i--) {
                string adjustorHash = AppModelConfig.convertToHex(contract.GetFunction("get").CallAsync<byte[]>(i).Result);
                // Add the adjustorDetails to the list if a valid hash has been returned
                if (AppModelConfig.isEmptyHash(adjustorHash) == false) {
                    var adjustor = contract.GetFunction("dataStorage").CallDeserializingToObjectAsync<AdjustorDetail>(adjustorHash.HexToByteArray()).Result;
                    adjustor.Hash = adjustorHash;
                    list.Items.Add(adjustor);
                }
            }
            // Return the adjustor list
            return list;
        }

        public object Get(GetAdjustor request) {
            // Get the contract for the Adjustor by specifying the adjustor address
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.BOND, AppServices.GetEcosystemAdr(request.ContractAdr).AdjustorContractAdr);

            // AdjustorListEntry entry = contract.GetFunction("get").CallDeserializingToObjectAsync<AdjustorListEntry>(i).Result;
            // If no adjustor hash has been provided as part of the request get the corresponding hash that belongs to the provided idx
            if (request.Hash.IsEmpty() == true)
                request.Hash = AppModelConfig.convertToHex(contract.GetFunction("get").CallAsync<byte[]>(request.Idx).Result);

            // Retrieve the adjustor details from the Blockchain
            AdjustorDetail adjustor = contract.GetFunction("dataStorage").CallDeserializingToObjectAsync<AdjustorDetail>(request.Hash.HexToByteArray()).Result;
            // Set the adjustor hash to the requested has as specified in the request
            adjustor.Hash = request.Hash;
            adjustor.EventLogs = new List<AdjustorEventLog>();

            // If adjustor hash is set retrieve the logs for the adjustor
            if (AppModelConfig.isEmptyHash(adjustor.Hash) == false) {
                adjustor.EventLogs = ((AdjustorLogs)this.Get(
                    new GetAdjustorLogs {ContractAdr = request.ContractAdr, Hash = request.Hash})).EventLogs;
                // Just for the Adjustor specific event logs reverse the order to have the events in ascending order
                adjustor.EventLogs.Reverse();
            }

            // Return the adjustor
            return adjustor;
        }

        public object Get(GetAdjustorLogs request) {
            // Retrieve the block parameters
            (BlockParameter fromBlock, BlockParameter toBlock) = AppServices.getBlockParameterConfiguration(request.FromBlock, request.ToBlock, 
                (request.Hash.IsEmpty() == true) && (request.Owner.IsEmpty() == true) && (request.Info.IsEmpty() == true));

            // Create the filter variables for selecting only the requested log entries
            object[] ft1 = (request.Hash.IsEmpty() == true ? null : new object[]{ request.Hash.HexToByteArray() });
            object[] ft2 = (request.Owner.IsEmpty() == true ? null : new object[]{ request.Owner });
            object[] ft3 = (request.Info.IsEmpty() == true ? null : new object[1]);

            // Adjust the filterinpu for ft3 if a value has been provided
            if (request.Info.IsEmpty() == false) {
                if (request.Info.HasHexPrefix() == true)
                    ft3[0] = request.Info.HexToByteArray();
                else if (uint.TryParse(request.Info, out uint val) == true)
                    ft3[0] = val.ToString("X64").EnsureHexPrefix().HexToByteArray();
            }

            // Retrieve the contract info
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.ADJUSTOR, AppServices.GetEcosystemAdr(request.ContractAdr).AdjustorContractAdr);
            
            // Create the filter input to extract the requested log entries
            var filterInput = contract.GetEvent("LogAdjustor").CreateFilterInput(filterTopic1: ft1, filterTopic2: ft2, filterTopic3: ft3, fromBlock: fromBlock, toBlock: toBlock);
            
            // Extract all the logs as specified by the filter input
            var res = AppServices.web3.Eth.Filters.GetLogs.SendRequestAsync(filterInput).Result;

            // Create the return instance
            var logs = new AdjustorLogs() { EventLogs = new List<AdjustorEventLog>() };

            // Interate through all the returned logs and add them to the logs list
            for (int i=res.Length - 1; i>=0; i--) {
                var log = new AdjustorEventLog();
                log.BlockNumber = Convert.ToUInt64(res[i].BlockNumber.HexValue, 16);
                log.Hash = res[i].Topics[1].ToString();        
                log.Owner = AppModelConfig.getAdrFromString32(res[i].Topics[2].ToString());
                log.Timestamp = Convert.ToUInt64(res[i].Data.Substring(2 + 0 * 64, 64), 16);

                if (AppModelConfig.isEmptyHash(res[i].Topics[3].ToString()) == true)
                    log.Info = "";
                else if (res[i].Topics[3].ToString().StartsWith("0x000000") == true)
                    log.Info = Convert.ToInt64(res[i].Topics[3].ToString(), 16).ToString();
                else log.Info = res[i].Topics[3].ToString();
                logs.EventLogs.Add(log);
            }

            // Return the list of adjustor logs
            return logs;
        }

        public object Post(CreateAdjustor request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.TRUST,
                AppServices.GetEcosystemAdr(request.ContractAdr).TrustContractAdr,
                request.SigningPrivateKey,
                "createAdjustor",
                request.Owner,
                request.SettlementApprovalAmount,
                request.PolicyRiskPointLimit,
                request.ServiceAgreementHash.HexToByteArray()
            );
        }

        public object Put(UpdateAdjustor request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.TRUST,
                AppServices.GetEcosystemAdr(request.ContractAdr).TrustContractAdr,
                request.SigningPrivateKey,
                "updateAdjustor",
                request.AdjustorHash.HexToByteArray(),
                request.Owner,
                request.SettlementApprovalAmount,
                request.PolicyRiskPointLimit,
                request.ServiceAgreementHash.HexToByteArray()
            );
        }

        public object Put(RetireAdjustor request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.TRUST,
                AppServices.GetEcosystemAdr(request.ContractAdr).TrustContractAdr,
                request.SigningPrivateKey,
                "retireAdjustor",
                request.AdjustorHash.HexToByteArray()
            );
        }
    }
}