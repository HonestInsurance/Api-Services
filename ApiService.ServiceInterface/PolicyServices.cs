/**
 * @description Policy services class implementing the api logic
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
    public class PolicyServices : Service
    {
        public object Get(GetPolicyList request) {
            // Get the contract for the Policy by specifying the Policy address
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.POLICY,  AppServices.GetEcosystemAdr(request.ContractAdr).PolicyContractAdr);

            // Create the Policy list object and initialise
            PolicyList list = new PolicyList() { Items = new List<PolicyDetail>() };

            // If no Policy Owner Address has been specified return all the Policys starting from lastIdx in decending order
            if (request.Owner.IsEmpty() == true) {
                // Get the metadata info for the first, next and count of Policys
                list.Info = contract.GetFunction("hashMap").CallDeserializingToObjectAsync<ListInfo>().Result;
                // Create the idx to continue the search from
                int lastIdx = (request.FromIdx != 0 ? (int)request.FromIdx :(int)list.Info.CountAllItems);
                // Define the lower bound
                int lowerBoundIdx = Math.Max(0, lastIdx - 
                    (request.MaxEntries != 0 ? (int)request.MaxEntries : (int)AppModelConfig.defaultNumberEntriesForLazyLoading));
                // Iterate through all the possilbe Policy entries
                for (int i=lastIdx; i>lowerBoundIdx; i--) {
                    string PolicyHash = AppModelConfig.convertToHex(contract.GetFunction("get").CallAsync<byte[]>(i).Result);
                    // Add the PolicyDetails to the list if a valid hash has been returned
                    if (AppModelConfig.isEmptyHash(PolicyHash) == false) {
                        var Policy = contract.GetFunction("dataStorage").CallDeserializingToObjectAsync<PolicyDetail>(PolicyHash.HexToByteArray()).Result;
                        Policy.Hash = PolicyHash;
                        list.Items.Add(Policy);
                    }
                }
            }
            else {
                // Get all the log files that match the Policy owner's address specified
                List<PolicyLog> logs = ((PolicyLogs)this.Get(
                    new GetPolicyLogs { ContractAdr = request.ContractAdr, Owner = request.Owner })).Logs;
                // Filter the list only for Created state entries and sort it with timestamp desc
                var filteredList = logs.GroupBy(x => x.Hash).Select(x => x.FirstOrDefault()).ToList().OrderByDescending(o=>o.Timestamp).ToList();
                // Iterate through the list
                for (int i=0; i<filteredList.Count; i++) {
                    var Policy = contract.GetFunction("dataStorage").CallDeserializingToObjectAsync<PolicyDetail>(filteredList[i].Hash.HexToByteArray()).Result;
                    Policy.Hash = filteredList[i].Hash;
                    list.Items.Add(Policy);
                }
            }

            // Return the Policy list
            return list;
        }

        public object Get(GetPolicy request) {
            // Get the contract for the Policy by specifying the Policy address
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.POLICY, AppServices.GetEcosystemAdr(request.ContractAdr).PolicyContractAdr);

            // PolicyListEntry entry = contract.GetFunction("get").CallDeserializingToObjectAsync<PolicyListEntry>(i).Result;
            // If no Policy hash has been provided as part of the request get the corresponding hash that belongs to the provided idx
            if (request.Hash.IsEmpty() == true)
                request.Hash = AppModelConfig.convertToHex(contract.GetFunction("get").CallAsync<byte[]>(request.Idx).Result);

            // Retrieve the Policy details from the Blockchain
            PolicyDetail Policy = contract.GetFunction("dataStorage").CallDeserializingToObjectAsync<PolicyDetail>(request.Hash.HexToByteArray()).Result;
            // Set the Policy hash to the requested has as specified in the request
            Policy.Hash = request.Hash;
            Policy.Logs = new List<PolicyLog>();

            // If Policy hash is set retrieve the logs for the Policy
            if (AppModelConfig.isEmptyHash(Policy.Hash) == false) {
                Policy.Logs = ((PolicyLogs)this.Get(
                    new GetPolicyLogs {ContractAdr = request.ContractAdr, Hash = request.Hash})).Logs;
                // Just for the Policy specific event logs reverse the order to have the events in ascending order
                Policy.Logs.Reverse();
            }

            // Return the Policy
            return Policy;
        }

        public object Get(GetPolicyLogs request) {
            // Return the requested log file entries
            return new PolicyLogs() { Logs = new LogParser<PolicyLog>().parseLogs(
                AppModelConfig.POLICY,
                AppServices.GetEcosystemAdr(request.ContractAdr).PolicyContractAdr,
                "LogPolicy",
                (request.Hash.IsEmpty() == true ? null : new object[]{ request.Hash.HexToByteArray() }),
                (request.Owner.IsEmpty() == true ? null : new object[]{ request.Owner }),
                (request.Info.IsEmpty() == true ? null : new object[1]),
                request.FromBlock,
                request.ToBlock,
                (request.Hash.IsEmpty() == true) && (request.Owner.IsEmpty() == true) && (request.Info.IsEmpty() == true)
            )};
        }

        public object Post(CreatePolicy request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.POLICY,
                AppServices.GetEcosystemAdr(request.ContractAdr).PolicyContractAdr,
                request.SigningPrivateKey,
                "createPolicy",
                request.AdjustorHash.HexToByteArray(),
                request.Owner,
                request.DocumentHash.HexToByteArray(),
                request.RiskPoints
            );
        }

        public object Put(UpdatePolicy request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.POLICY,
                AppServices.GetEcosystemAdr(request.ContractAdr).PolicyContractAdr,
                request.SigningPrivateKey,
                "updatePolicy",
                request.AdjustorHash.HexToByteArray(),
                request.PolicyHash.HexToByteArray(),
                request.DocumentHash.HexToByteArray(),
                request.RiskPoints
            );
        }

        public object Put(SuspendPolicy request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.POLICY,
                AppServices.GetEcosystemAdr(request.ContractAdr).PolicyContractAdr,
                request.SigningPrivateKey,
                "suspendPolicy",
                request.PolicyHash.HexToByteArray()
            );
        }

        public object Put(UnsuspendPolicy request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.POLICY,
                AppServices.GetEcosystemAdr(request.ContractAdr).PolicyContractAdr,
                request.SigningPrivateKey,
                "unsuspendPolicy",
                request.PolicyHash.HexToByteArray()
            );
        }

        public object Put(RetirePolicy request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.POLICY,
                AppServices.GetEcosystemAdr(request.ContractAdr).PolicyContractAdr,
                request.SigningPrivateKey,
                "retirePolicy",
                request.PolicyHash.HexToByteArray()
            );
        }
    }
}