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
            // Return the requested log file entries
            return new BankLogs() { Logs = new LogParser<BankLog>().parseLogs(
                AppModelConfig.BANK,
                AppServices.GetEcosystemAdr(request.ContractAdr).BankContractAdr,
                "LogBank",
                (request.InternalReferenceHash.IsEmpty() == true ? null : new object[]{ request.InternalReferenceHash.HexToByteArray() }),
                new object[]{(uint)request.AccountType},
                (request.Success == SuccessFilter.All ? null : (request.Success == SuccessFilter.Positive ? new object[]{ true } : new object[]{ false })),
                request.FromBlock,
                request.ToBlock,
                (request.Success == SuccessFilter.All) && (request.InternalReferenceHash.IsEmpty() == true)
            )};
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