/**
 * @description Bond services class implementing the api logic
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
    public class BondServices : Service
    {
        public object Get(GetBondList request) {
            // Get the contract for the Bond by specifying the bond address
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.BOND,  AppServices.GetEcosystemAdr(request.ContractAdr).BondContractAdr);

            // Create the bond list object and initialise
            BondList list = new BondList() { Items = new List<BondDetail>() };

            // If no Bond Owner Address has been specified return all the bonds starting from lastIdx in decending order
            if (request.Owner.IsEmpty() == true) {
                // Get the metadata info for the first, next and count of Bonds
                list.Info = contract.GetFunction("hashMap").CallDeserializingToObjectAsync<ListInfo>().Result;
                // Create the idx to continue the search from
                int lastIdx = (request.FromIdx != 0 ? (int)request.FromIdx :(int)list.Info.CountAllItems);
                // Define the lower bound 
                int lowerBoundIdx = Math.Max(0, lastIdx - 
                    (request.MaxEntries != 0 ? (int)request.MaxEntries : (int)AppModelConfig.defaultNumberEntriesForLazyLoading));
                // Iterate through all the possilbe bond entries
                for (int i=lastIdx; i>lowerBoundIdx; i--) {
                    string bondHash = AppModelConfig.convertToHex(contract.GetFunction("get").CallAsync<byte[]>(i).Result);
                    // Add the bondDetails to the list if a valid hash has been returned
                    if (AppModelConfig.isEmptyHash(bondHash) == false) {
                        var bond = contract.GetFunction("dataStorage").CallDeserializingToObjectAsync<BondDetail>(bondHash.HexToByteArray()).Result;
                        bond.Hash = bondHash;
                        list.Items.Add(bond);
                    }
                }
            }
            else {
                // Get all the log files that match the bond owner's address specified
                List<BondEventLog> logs = ((BondLogs)this.Get(
                    new GetBondLogs { ContractAdr = request.ContractAdr, Owner = request.Owner })).EventLogs;
                // Filter the list only for Created state entries and sort it with timestamp desc
                var filteredList = logs.GroupBy(x => x.Hash).Select(x => x.FirstOrDefault()).ToList().OrderByDescending(o=>o.Timestamp).ToList();
                // Iterate through the list
                for (int i=0; i<filteredList.Count; i++) {
                    var bond = contract.GetFunction("dataStorage").CallDeserializingToObjectAsync<BondDetail>(filteredList[i].Hash.HexToByteArray()).Result;
                    bond.Hash = filteredList[i].Hash;
                    list.Items.Add(bond);
                }
            }

            // Return the bond list
            return list;
        }

        public object Get(GetBond request) {
            // Get the contract for the Bond by specifying the bond address
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.BOND, AppServices.GetEcosystemAdr(request.ContractAdr).BondContractAdr);

            // BondListEntry entry = contract.GetFunction("get").CallDeserializingToObjectAsync<BondListEntry>(i).Result;
            // If no bond hash has been provided as part of the request get the corresponding hash that belongs to the provided idx
            if (request.Hash.IsEmpty() == true)
                request.Hash = AppModelConfig.convertToHex(contract.GetFunction("get").CallAsync<byte[]>(request.Idx).Result);

            // Retrieve the bond details from the Blockchain
            BondDetail bond = contract.GetFunction("dataStorage").CallDeserializingToObjectAsync<BondDetail>(request.Hash.HexToByteArray()).Result;
            // Set the bond hash to the requested has as specified in the request
            bond.Hash = request.Hash;
            bond.EventLogs = new List<BondEventLog>();

            // If bond hash is set retrieve the logs for the bond
            if (AppModelConfig.isEmptyHash(bond.Hash) == false) {
                bond.EventLogs = ((BondLogs)this.Get(
                    new GetBondLogs {ContractAdr = request.ContractAdr, Hash = request.Hash})).EventLogs;
                // Just for the Bond specific event logs reverse the order to have the events in ascending order
                bond.EventLogs.Reverse();
            }

            // Return the bond
            return bond;
        }

        public object Get(GetBondLogs request) {
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
                else ft3[0] = AppModelConfig.convertToHex64(request.Info).HexToByteArray();
            }

            // Retrieve the contract info
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.BOND, AppServices.GetEcosystemAdr(request.ContractAdr).BondContractAdr);
            
            // Create the filter input to extract the requested log entries
            var filterInput = contract.GetEvent("LogBond").CreateFilterInput(filterTopic1: ft1, filterTopic2: ft2, filterTopic3: ft3, fromBlock: fromBlock, toBlock: toBlock);
            
            // Extract all the logs as specified by the filter input
            var res = AppServices.web3.Eth.Filters.GetLogs.SendRequestAsync(filterInput).Result;

            // Create the return instance
            var logs = new BondLogs() { EventLogs = new List<BondEventLog>() };

            // Interate through all the returned logs and add them to the logs list
            for (int i=res.Length - 1; i>=0; i--) {
                var log = new BondEventLog();
                log.BlockNumber = Convert.ToUInt64(res[i].BlockNumber.HexValue, 16);
                log.Hash = res[i].Topics[1].ToString();        
                log.Owner = AppModelConfig.getAdrFromString32(res[i].Topics[2].ToString());
                log.Timestamp = Convert.ToUInt64(res[i].Data.Substring(2 + 0 * 64, 64), 16);
                log.State = (BondState)Convert.ToInt32(res[i].Data.Substring(2 + 1 * 64,64), 16);
                if (AppModelConfig.isEmptyHash(log.Hash))
                    log.Info = AppModelConfig.FromHexString(res[i].Topics[3].ToString());
                else if ((log.State == BondState.SecuredReferenceBond) || (log.State == BondState.LockedReferenceBond))
                    log.Info = res[i].Topics[3].ToString().EnsureHexPrefix();
                else log.Info = Convert.ToInt64(res[i].Topics[3].ToString(), 16).ToString();
                logs.EventLogs.Add(log);
            }

            // Return the list of bond logs
            return logs;
        }

        public object Post(CreateBond request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.BOND,
                AppServices.GetEcosystemAdr(request.ContractAdr).BondContractAdr,
                request.SigningPrivateKey,
                "createBond",
                request.Principal,
                (AppModelConfig.isEmptyHash(request.SecurityReferenceHash) ? AppModelConfig.EMPTY_HASH.HexToByteArray() : request.SecurityReferenceHash.HexToByteArray())
            );
        }
    }
}