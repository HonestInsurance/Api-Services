/**
 * @description Settlement services class implementing the api logic
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
    public class SettlementServices : Service
    {
        public object Get(GetSettlementList request) {
            // Get the contract for the Settlement by specifying the settlement address
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.SETTLEMENT,  AppServices.GetEcosystemAdr(request.ContractAdr).SettlementContractAdr);

            // Create the settlement list object and initialise
            SettlementList list = new SettlementList() { Items = new List<SettlementDetail>() };

            // Get the metadata info for the first, next and count of Settlements
            list.Info = contract.GetFunction("hashMap").CallDeserializingToObjectAsync<ListInfo>().Result;
            // Create the idx to continue the search from
            int lastIdx = (request.FromIdx != 0 ? (int)request.FromIdx :(int)list.Info.CountAllItems);
            // Define the lower bound 
            int lowerBoundIdx = Math.Max(0, lastIdx - 
                (request.MaxEntries != 0 ? (int)request.MaxEntries : (int)AppModelConfig.defaultNumberEntriesForLazyLoading));
            // Iterate through all the possilbe settlement entries
            for (int i=lastIdx; i>lowerBoundIdx; i--) {
                string settlementHash = AppModelConfig.convertToHex(contract.GetFunction("get").CallAsync<byte[]>(i).Result);
                // Add the settlementDetails to the list if a valid hash has been returned
                if (AppModelConfig.isEmptyHash(settlementHash) == false) {
                    var settlement = contract.GetFunction("dataStorage").CallDeserializingToObjectAsync<SettlementDetail>(settlementHash.HexToByteArray()).Result;
                    settlement.Hash = settlementHash;
                    list.Items.Add(settlement);
                }
            }
            // Return the settlement list
            return list;
        }

        public object Get(GetSettlement request) {
            // Get the contract for the Settlement by specifying the settlement address
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.SETTLEMENT, AppServices.GetEcosystemAdr(request.ContractAdr).SettlementContractAdr);

            // SettlementListEntry entry = contract.GetFunction("get").CallDeserializingToObjectAsync<SettlementListEntry>(i).Result;
            // If no settlement hash has been provided as part of the request get the corresponding hash that belongs to the provided idx
            if (request.Hash.IsEmpty() == true)
                request.Hash = AppModelConfig.convertToHex(contract.GetFunction("get").CallAsync<byte[]>(request.Idx).Result);

            // Retrieve the settlement details from the Blockchain
            SettlementDetail settlement = contract.GetFunction("dataStorage").CallDeserializingToObjectAsync<SettlementDetail>(request.Hash.HexToByteArray()).Result;
            // Set the settlement hash to the requested has as specified in the request
            settlement.Hash = request.Hash;
            settlement.EventLogs = new List<SettlementEventLog>();

            // If settlement hash is set retrieve the logs for the settlement
            if (AppModelConfig.isEmptyHash(settlement.Hash) == false) {
                settlement.EventLogs = ((SettlementLogs)this.Get(
                    new GetSettlementLogs {ContractAdr = request.ContractAdr, SettlementHash = request.Hash})).EventLogs;
                // Just for the Settlement specific event logs reverse the order to have the events in ascending order
                settlement.EventLogs.Reverse();
            }

            // Return the settlement
            return settlement;
        }

        public object Get(GetSettlementLogs request) {
            // Retrieve the block parameters
            (BlockParameter fromBlock, BlockParameter toBlock) = AppServices.getBlockParameterConfiguration(request.FromBlock, request.ToBlock, 
                (request.SettlementHash.IsEmpty() == true) && (request.AdjustorHash.IsEmpty() == true) && (request.Info.IsEmpty() == true));

            // Create the filter variables for selecting only the requested log entries
            object[] ft1 = (request.SettlementHash.IsEmpty() == true ? null : new object[]{ request.SettlementHash.HexToByteArray() });
            object[] ft2 = (request.AdjustorHash.IsEmpty() == true ? null : new object[]{ request.AdjustorHash.HexToByteArray() });
            object[] ft3 = (request.Info.IsEmpty() == true ? null : new object[]{ request.Info.HexToByteArray() });

            // Retrieve the contract info
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.SETTLEMENT, AppServices.GetEcosystemAdr(request.ContractAdr).SettlementContractAdr);
            
            // Create the filter input to extract the requested log entries
            var filterInput = contract.GetEvent("LogSettlement").CreateFilterInput(filterTopic1: ft1, filterTopic2: ft2, filterTopic3: ft3, fromBlock: fromBlock, toBlock: toBlock);
            
            // Extract all the logs as specified by the filter input
            var res = AppServices.web3.Eth.Filters.GetLogs.SendRequestAsync(filterInput).Result;

            // Create the return instance
            var logs = new SettlementLogs() { EventLogs = new List<SettlementEventLog>() };

            // Interate through all the returned logs and add them to the logs list
            for (int i=res.Length - 1; i>=0; i--) {
                var log = new SettlementEventLog();
                log.BlockNumber = Convert.ToUInt64(res[i].BlockNumber.HexValue, 16);
                log.SettlementHash = res[i].Topics[1].ToString();
                log.AdjustorHash = res[i].Topics[2].ToString();
                log.Info = res[i].Topics[3].ToString();
                log.Timestamp = Convert.ToUInt64(res[i].Data.Substring(2 + 0 * 64, 64), 16);
                log.State = (SettlementState)Convert.ToInt32(res[i].Data.Substring(2 + 1 * 64,64), 16);
                logs.EventLogs.Add(log);
            }

            // Return the list of settlement logs
            return logs;
        }

        public object Post(CreateSettlement request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.SETTLEMENT,
                AppServices.GetEcosystemAdr(request.ContractAdr).SettlementContractAdr,
                request.SigningPrivateKey,
                "createSettlement",
                request.AdjustorHash.HexToByteArray(),
                (AppModelConfig.isEmptyHash(request.PolicyHash) ? AppModelConfig.EMPTY_HASH.HexToByteArray() : request.PolicyHash.HexToByteArray()),
                (AppModelConfig.isEmptyHash(request.DocumentHash) ? AppModelConfig.EMPTY_HASH.HexToByteArray() : request.DocumentHash.HexToByteArray())
            );
        }

        public object Put(AddSettlementInfo request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.SETTLEMENT,
                AppServices.GetEcosystemAdr(request.ContractAdr).SettlementContractAdr,
                request.SigningPrivateKey,
                "addSettlementInfo",
                request.SettlementHash.HexToByteArray(),
                request.AdjustorHash.HexToByteArray(),
                request.DocumentHash.HexToByteArray()
            );
        }

        public object Put(CloseSettlement request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.SETTLEMENT,
                AppServices.GetEcosystemAdr(request.ContractAdr).SettlementContractAdr,
                request.SigningPrivateKey,
                "closeSettlement",
                request.SettlementHash.HexToByteArray(),
                request.AdjustorHash.HexToByteArray(),
                request.DocumentHash.HexToByteArray(),
                request.SettlementAmount
            );
        }

        public object Put(SetExpectedSettlementAmount request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.SETTLEMENT,
                AppServices.GetEcosystemAdr(request.ContractAdr).SettlementContractAdr,
                request.SigningPrivateKey,
                "setExpectedSettlementAmount",
                request.SettlementHash.HexToByteArray(),
                request.AdjustorHash.HexToByteArray(),
                request.ExpectedSettlementAmount
            );
        }
    }
}