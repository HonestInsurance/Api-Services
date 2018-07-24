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

        public object Post(DeployContracts request) {
            // Deploy the library contract and await until the transaction has been mined
            TransactionHash libTransactionHash = AppServices.createSignDeployContract(
                AppModelConfig.LIB.abi, 
                AppModelConfig.LIB.bytecode, 
                request.SigningPrivateKey, 
                null);
            TransactionResult libTransResult = (TransactionResult)Get(new GetReceipt { TransactionHash = libTransactionHash.Hash, MaxWaitDurationForTransactionReceipt = 300});
            
            // Deploy the trust contract and await until the transaction has been mined
            TransactionHash trustTransactionHash = AppServices.createSignDeployContract(
                AppModelConfig.TRUST.abi, 
                AppModelConfig.linkContractBytecode(AppModelConfig.TRUST.bytecode, "Lib", libTransResult.ContractAddress), 
                request.SigningPrivateKey,
                null);
            TransactionResult trustTransResult = (TransactionResult)Get(new GetReceipt { TransactionHash = trustTransactionHash.Hash, MaxWaitDurationForTransactionReceipt = 300});

            // Deploy the pool contract
            TransactionHash poolTransactionHash = AppServices.createSignDeployContract(
                AppModelConfig.POOL.abi, 
                AppModelConfig.linkContractBytecode(AppModelConfig.POOL.bytecode, "Lib", libTransResult.ContractAddress), 
                request.SigningPrivateKey,
                trustTransResult.ContractAddress);
            TransactionResult poolTransResult = (TransactionResult)Get(new GetReceipt { TransactionHash = poolTransactionHash.Hash, MaxWaitDurationForTransactionReceipt = 300});
            
            // Deploy the bond contract
            TransactionHash bondTransactionHash = AppServices.createSignDeployContract(
                AppModelConfig.BOND.abi, 
                AppModelConfig.linkContractBytecode(AppModelConfig.BOND.bytecode, "Lib", libTransResult.ContractAddress), 
                request.SigningPrivateKey,
                trustTransResult.ContractAddress);
            TransactionResult bondTransResult = (TransactionResult)Get(new GetReceipt { TransactionHash = bondTransactionHash.Hash, MaxWaitDurationForTransactionReceipt = 300});
            
            // Deploy the bank contract
            TransactionHash bankTransactionHash = AppServices.createSignDeployContract(
                AppModelConfig.BANK.abi, 
                AppModelConfig.linkContractBytecode(AppModelConfig.BANK.bytecode, "Lib", libTransResult.ContractAddress), 
                request.SigningPrivateKey,
                trustTransResult.ContractAddress);
            TransactionResult bankTransResult = (TransactionResult)Get(new GetReceipt { TransactionHash = bankTransactionHash.Hash, MaxWaitDurationForTransactionReceipt = 300});

            // Deploy the policy contract
            TransactionHash policyTransactionHash = AppServices.createSignDeployContract(
                AppModelConfig.POLICY.abi, 
                AppModelConfig.linkContractBytecode(AppModelConfig.POLICY.bytecode, "Lib", libTransResult.ContractAddress), 
                request.SigningPrivateKey,
                trustTransResult.ContractAddress);
            TransactionResult policyTransResult = (TransactionResult)Get(new GetReceipt { TransactionHash = policyTransactionHash.Hash, MaxWaitDurationForTransactionReceipt = 300});
            
            // Deploy the claim contract
            TransactionHash settlementTransactionHash = AppServices.createSignDeployContract(
                AppModelConfig.SETTLEMENT.abi, 
                AppModelConfig.linkContractBytecode(AppModelConfig.SETTLEMENT.bytecode, "Lib", libTransResult.ContractAddress), 
                request.SigningPrivateKey,
                trustTransResult.ContractAddress);
            TransactionResult settlementTransResult = (TransactionResult)Get(new GetReceipt { TransactionHash = settlementTransactionHash.Hash, MaxWaitDurationForTransactionReceipt = 300});
            
            // Deploy the adjustor contract
            TransactionHash adjustorTransactionHash = AppServices.createSignDeployContract(
                AppModelConfig.ADJUSTOR.abi, 
                AppModelConfig.linkContractBytecode(AppModelConfig.ADJUSTOR.bytecode, "Lib", libTransResult.ContractAddress), 
                request.SigningPrivateKey,
                trustTransResult.ContractAddress);
            TransactionResult adjustorTransResult = (TransactionResult)Get(new GetReceipt { TransactionHash = adjustorTransactionHash.Hash, MaxWaitDurationForTransactionReceipt = 300});
            
            // Deploy the timer contract
            TransactionHash timerTransactionHash = AppServices.createSignDeployContract(
                AppModelConfig.TIMER.abi, 
                AppModelConfig.linkContractBytecode(AppModelConfig.TIMER.bytecode, "Lib", libTransResult.ContractAddress), 
                request.SigningPrivateKey,
                trustTransResult.ContractAddress);
            TransactionResult timerTransResult = (TransactionResult)Get(new GetReceipt { TransactionHash = timerTransactionHash.Hash, MaxWaitDurationForTransactionReceipt = 300});

            // Submit and return the transaction hash of the broadcasted transaction
            TransactionHash initEcosytemHash = AppServices.createSignPublishTransaction(
                AppModelConfig.TRUST.abi, 
                trustTransResult.ContractAddress, 
                request.SigningPrivateKey,
                "initEcosystem",
                poolTransResult.ContractAddress,            // pool
                bondTransResult.ContractAddress,            // bond
                bankTransResult.ContractAddress,            // bank
                policyTransResult.ContractAddress,          // policy
                settlementTransResult.ContractAddress,      // settlement
                adjustorTransResult.ContractAddress,        // adjustor
                timerTransResult.ContractAddress,           // timer
                request.IsWinterTime
            );

            // Get the transaction result
            TransactionResult initTransResult = (TransactionResult)Get(new GetReceipt { TransactionHash = initEcosytemHash.Hash, MaxWaitDurationForTransactionReceipt = 300});

            // Return the addresses of the newly created contracts
            return new EcosystemContractAddresses {
                TrustContractAdr = trustTransResult.ContractAddress,
                PoolContractAdr = poolTransResult.ContractAddress,
                BondContractAdr = bondTransResult.ContractAddress,
                BankContractAdr = bankTransResult.ContractAddress,
                PolicyContractAdr = policyTransResult.ContractAddress,
                SettlementContractAdr = settlementTransResult.ContractAddress,
                AdjustorContractAdr = adjustorTransResult.ContractAddress,
                TimerContractAdr = timerTransResult.ContractAddress
            };
        }
    }
}