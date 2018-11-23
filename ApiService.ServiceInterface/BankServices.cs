/**
 * @description Bank services class implementing the api logic
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
    public class BankServices : Service
    {
        public object Get(GetBankLogs request) {
            // Retrieve the block parameters
            (BlockParameter fromBlock, BlockParameter toBlock) = AppServices.getBlockParameterConfiguration(request.FromBlock, request.ToBlock, 
                (request.Success == SuccessFilter.All) && (request.InternalReferenceHash.IsEmpty() == true));

            // Create the filter variables for selecting only the requested log entries
            object[] ft1 = (request.InternalReferenceHash.IsEmpty() == true ? null : new object[]{ request.InternalReferenceHash.HexToByteArray() });
            object[] ft2 = {(uint)request.AccountType};
            object[] ft3 = (request.Success == SuccessFilter.All ? null : (request.Success == SuccessFilter.Positive ? new object[]{ true } : new object[]{ false }));

            // Retrieve the contract info
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.BANK, AppServices.GetEcosystemAdr(request.ContractAdr).BankContractAdr);
            
            // Create the filter input to extract the requested log entries
            var filterInput = contract.GetEvent("LogBank").CreateFilterInput(filterTopic1: ft1, filterTopic2: ft2, filterTopic3: ft3, fromBlock: fromBlock, toBlock: toBlock);
            
            // Extract all the logs as specified by the filter input
            var res = AppServices.web3.Eth.Filters.GetLogs.SendRequestAsync(filterInput).Result;

            // Create the return instance
            var logs = new BankLogs() { EventLogs = new List<BankEventLog>() };

            // Interate through all the returned logs and add them to the logs list
            for (int i=res.Length - 1; i>=0; i--) {
                var log = new BankEventLog();
                log.BlockNumber = Convert.ToUInt64(res[i].BlockNumber.HexValue, 16);
                log.InternalReferenceHash = res[i].Topics[1].ToString();
                log.AccountType = (AccountType)Convert.ToUInt64(res[i].Topics[2].ToString(), 16);
                log.Success = res[i].Topics[3].ToString().EndsWith("1");
                log.PaymentAccountHash = res[i].Data.Substring(2 + 0 * 64, 64).EnsureHexPrefix();
                log.PaymentSubject = res[i].Data.Substring(2 + 1 * 64, 64).EnsureHexPrefix().StartsWith("0x000000") ?
                    Convert.ToUInt64(res[i].Data.Substring(2 + 1 * 64, 64), 16).ToString() :
                    res[i].Data.Substring(2 + 1 * 64, 64).EnsureHexPrefix();
                log.Info = AppModelConfig.isEmptyHash(res[i].Data.Substring(2 + 2 * 64, 64).EnsureHexPrefix()) ? "0x0" : AppModelConfig.FromHexString(res[i].Data.Substring(2 + 2 * 64, 64));
                log.Timestamp = Convert.ToUInt64(res[i].Data.Substring(2 + 3 * 64, 64), 16);
                log.TransactionType = (TransactionType)Convert.ToUInt64(res[i].Data.Substring(2 + 4 * 64, 64), 16);
                log.Amount = Convert.ToUInt64(res[i].Data.Substring(2 + 5 * 64, 64), 16);
                logs.EventLogs.Add(log);
            }

            // Return the list of bond logs
            return logs;
        }

        public object Get(GetBankPaymentAdviceList request) {
            // Get the contract for the Bank by specifying the bank address
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.BANK, AppServices.GetEcosystemAdr(request.ContractAdr).BankContractAdr);

            // Create a new return instance for Bank Metadata and set the Bank contract address
            uint countPaymentAdvice = contract.GetFunction("countPaymentAdviceEntries").CallAsync<uint>().Result;

            // Create the bond list object and initialise
            PaymentAdviceList list = new PaymentAdviceList() { Items = new List<PaymentAdviceDetail>() };
            
            // Iterate through all the payment advice entries available according to count
            for (uint i=0; i<countPaymentAdvice; i++) {
                // Call the payment advice from the specified idx
                PaymentAdviceDetail advice = contract.GetFunction("bankPaymentAdvice").CallDeserializingToObjectAsync<PaymentAdviceDetail>(i).Result;
                // Verify the payment advice returned has not already been processed (check the payment amount)
                if (advice.Amount > 0) {
                    // Set the Advice index
                    advice.Idx = i;
                    // Convert the payment subject if applicable
                    if (advice.PaymentSubject.StartsWith("0x000000") == true)
                        advice.PaymentSubject = Convert.ToUInt64(advice.PaymentSubject.RemoveHexPrefix(), 16).ToString();
                    // Add advice to the list
                    list.Items.Add(advice);
                }
            }
            // Return the list of all outstanding payment advice
            return list;
        }

        public object Post(ProcessBankPaymentAdvice request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.BANK, 
                AppServices.GetEcosystemAdr(request.ContractAdr).BankContractAdr,
                request.SigningPrivateKey,
                "processPaymentAdvice",
                request.AdviceIdx,
                request.BankTransactionIdx
            );
        }

        public object Post(ProcessBankAccountCredit request) {
            // Convert the payment subject to a hash if it is a valid number
            if (uint.TryParse(request.PaymentSubject, out uint val) == true)
                request.PaymentSubject = val.ToString("X64").EnsureHexPrefix();

            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.BANK,
                AppServices.GetEcosystemAdr(request.ContractAdr).BankContractAdr,
                request.SigningPrivateKey,
                "processAccountCredit",
                request.BankTransactionIdx,
                (ulong)request.AccountType,
                request.PaymentAccountHashSender.HexToByteArray(),
                request.PaymentSubject.HexToByteArray(),
                request.CreditAmount
            );
        }
    }
}