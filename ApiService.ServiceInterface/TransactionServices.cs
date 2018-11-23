/**
 * @description Transaction services class implementing the api logic
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
    public class TransactionServices : Service 
    {    
        public object Get(GetReceipt request) {
            // Retrieve the receipt
            var receipt = AppServices.web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(request.TransactionHash).Result;
            
            // In case a max wait duration has been specified use that duration, otherwise use the default wait duration
            ulong maxWaitDuration = (request.MaxWaitDurationForTransactionReceipt > 0 ? 
                request.MaxWaitDurationForTransactionReceipt : 
                AppModelConfig.maxWaitDurationForTransactionReceipt);
            // Variable to count wait duration
            ulong totalWaitDuration = 0;
            while ((receipt == null) && (totalWaitDuration < maxWaitDuration)) {
                Thread.Sleep(1000);
                totalWaitDuration++;
                receipt = AppServices.web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(request.TransactionHash).Result;
            }

            // Verify if a valid receipt has been returned - if not return NotFund HTTP Error
            if (receipt == null)
                return new HttpError(HttpStatusCode.NotFound, "Specified transaction hash could not be found or the specified transaction has not been mined yet");
                //return new HttpResult(404, "Specified transaction hash could not be found");

            // Create the transaction result instance and initialise it with values
            TransactionResult result = new TransactionResult {
                TransactionHash = receipt.TransactionHash,
                TransactionIndex = (uint)receipt.TransactionIndex.Value,
                BlockHash = receipt.BlockHash,
                BlockNumber = (uint)receipt.BlockNumber.Value,
                GasUsed = (uint)receipt.GasUsed.Value,
                CumulativeGasUsed = (uint)receipt.CumulativeGasUsed.Value,
                Success = (receipt.Status.Value == 1 ? true : false),
                ContractAddress = receipt.ContractAddress,
                LogCount = (uint)receipt.Logs.Count,
                ContractReferenceAdr = new List<string>(),
                ObjectReferenceHash = new List<string>()
            };

            // Add all the event log data that were created as part of the transaction
            for (int i=0; i<result.LogCount; i++) {
                result.ContractReferenceAdr.Add(receipt.Logs[i].Value<string>("address"));
                result.ObjectReferenceHash.Add(receipt.Logs[i].Value<Newtonsoft.Json.Linq.JArray>("topics")[1].ToString());
            }

            // Return the transaction result
            return result;
        }
    }
}