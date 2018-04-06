/**
 * @description Ecosystem wide services class implementing the api logic
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
    public class EcosystemServices : Service 
    {    
        // ********************************************************************************************
        // *** ECOSYSTEM
        // ********************************************************************************************

        public object Get(GetEcosystemStatus request) {
            // Using the request's contract address provided, retrieve all the ecosystem's addresses to get the pool address
            EcosystemContractAddresses adr = AppServices.GetEcosystemAdr(request.ContractAdr);

            // Create the return instance
            EcosystemStatus status = new EcosystemStatus();

            // Load the trust contract to retrieve the external access interface duration for pre-authorisation
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.POOL.abi, adr.PoolContractAdr);

            status.currentPoolDay = contract.GetFunction("currentPoolDay").CallAsync<ulong>().Result;
            status.isWinterTime = contract.GetFunction("isWinterTime").CallAsync<bool>().Result;
            status.daylightSavingScheduled = contract.GetFunction("daylightSavingScheduled").CallAsync<bool>().Result;

            // Bank account balances
            status.WC_Bal_FA_Cu = contract.GetFunction("WC_Bal_FA_Cu").CallAsync<ulong>().Result;
            status.WC_Bal_BA_Cu = contract.GetFunction("WC_Bal_BA_Cu").CallAsync<ulong>().Result;
            status.WC_Bal_PA_Cu = contract.GetFunction("WC_Bal_PA_Cu").CallAsync<ulong>().Result;

            // Variables used by the Insurance Pool during overnight processing only
            status.overwriteWcExpenses = contract.GetFunction("overwriteWcExpenses").CallAsync<bool>().Result;
            status.WC_Exp_Cu = contract.GetFunction("WC_Exp_Cu").CallAsync<ulong>().Result;
            
            // Pool variables as defined in the model
            status.WC_Locked_Cu = contract.GetFunction("WC_Locked_Cu").CallAsync<ulong>().Result;
            status.WC_Bond_Cu = contract.GetFunction("WC_Bond_Cu").CallAsync<ulong>().Result;
            status.WC_Transit_Cu = contract.GetFunction("WC_Transit_Cu").CallAsync<ulong>().Result;
            status.B_Yield_Ppb = contract.GetFunction("B_Yield_Ppb").CallAsync<ulong>().Result;
            status.B_Gradient_Ppq = contract.GetFunction("B_Gradient_Ppq").CallAsync<ulong>().Result;

            // Flag to indicate if the bond yield accelleration is operational (and scheduled by the timer)
            status.bondYieldAccellerationScheduled = contract.GetFunction("bondYieldAccellerationScheduled").CallAsync<bool>().Result;
            status.bondYieldAccelerationThreshold = contract.GetFunction("bondYieldAccelerationThreshold").CallAsync<ulong>().Result;

            // Load the bond contract
            contract = AppServices.web3.Eth.GetContract(AppModelConfig.BOND.abi, adr.BondContractAdr);
            status.BondListInfo = contract.GetFunction("hashMap").CallDeserializingToObjectAsync<ListInfo>().Result;

            // Load the policy contract
            contract = AppServices.web3.Eth.GetContract(AppModelConfig.POLICY.abi, adr.PolicyContractAdr);
            status.totalIssuedPolicyRiskPoints = contract.GetFunction("totalIssuedPolicyRiskPoints").CallAsync<ulong>().Result;
            status.PolicyListInfo = contract.GetFunction("hashMap").CallDeserializingToObjectAsync<ListInfo>().Result;

            // Load the adjustor contract
            contract = AppServices.web3.Eth.GetContract(AppModelConfig.ADJUSTOR.abi, adr.AdjustorContractAdr);
            status.AdjustorListInfo = contract.GetFunction("hashMap").CallDeserializingToObjectAsync<ListInfo>().Result;

            // Load the settlement contract
            contract = AppServices.web3.Eth.GetContract(AppModelConfig.SETTLEMENT.abi, adr.SettlementContractAdr);
            status.SettlementListInfo = contract.GetFunction("hashMap").CallDeserializingToObjectAsync<ListInfo>().Result;

            // Load the bank contract
            contract = AppServices.web3.Eth.GetContract(AppModelConfig.BANK.abi, adr.BankContractAdr);
            status.fundingAccountPaymentsTracking_Cu = contract.GetFunction("fundingAccountPaymentsTracking_Cu").CallAsync<ulong>().Result;

            // Load the timer contract
            contract = AppServices.web3.Eth.GetContract(AppModelConfig.TIMER.abi, adr.TimerContractAdr);
            status.lastPingExececution = contract.GetFunction("lastPingExec_10_S").CallAsync<ulong>().Result * 10;

            return status;
        }

        public object Get(GetEcosystemLogs request) {
            // Retrieve the block parameters
            (BlockParameter fromBlock, BlockParameter toBlock) = AppServices.getBlockParameterConfiguration(request.FromBlock, request.ToBlock, 
                (request.Subject.IsEmpty() == true) && (request.Day == 0) && (request.Value == 0));

            // Create the filter variables for selecting only the requested log entries
            object[] ft1 = (request.Subject.IsEmpty() == true ? null : new object[]{ AppModelConfig.convertToHex64(request.Subject).HexToByteArray() });
            object[] ft2 = (request.Day == 0 ? null : new object[]{ request.Day });
            object[] ft3 = (request.Value == 0 ? null : new object[]{ request.Value });

            // Retrieve the contract info
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.POOL.abi, AppServices.GetEcosystemAdr(request.ContractAdr).PoolContractAdr);
            
            // Create the filter input to extract the requested log entries
            var filterInput = contract.GetEvent("LogPool").CreateFilterInput(filterTopic1: ft1, filterTopic2: ft2, filterTopic3: ft3, fromBlock: fromBlock, toBlock: toBlock);
            
            // Extract all the logs as specified by the filter input
            var res = AppServices.web3.Eth.Filters.GetLogs.SendRequestAsync(filterInput).Result;

            // Create the return instance
            var logs = new EcosystemLogs() { EventLogs = new List<EcosystemEventLog>() };

            // Interate through all the returned logs and add them to the logs list
            for (int i=res.Length - 1; i>=0; i--) {
                logs.EventLogs.Add(new EcosystemEventLog() {
                    BlockNumber = Convert.ToUInt64(res[i].BlockNumber.HexValue, 16),
                    Subject = AppModelConfig.FromHexString(res[i].Topics[1].ToString()),            
                    Day = Convert.ToUInt64(res[i].Topics[2].ToString(), 16),
                    Value = Convert.ToUInt64(res[i].Topics[3].ToString(), 16),
                    Timestamp = Convert.ToUInt64(res[i].Data.Substring(2 + 0 * 64, 64), 16)
                });
            }

            // Return the list of bond logs
            return logs;
        }

        public object Get(GetEcosystemConfiguration request) {
            // Using the request's contract address provided, retrieve all the ecosystem's addresses to get the pool address
            EcosystemContractAddresses adr = AppServices.GetEcosystemAdr(request.ContractAdr);

            // Create the return instance
            EcosystemConfiguration setup = new EcosystemConfiguration();

            // Load the trust contract to retrieve the external access interface duration for pre-authorisation
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.EXTACCESSI.abi, adr.TrustContractAdr);
            // Pre-authorisation duration for external authentication
            setup.EXT_ACCESS_PRE_AUTH_DURATION_SEC = contract.GetFunction("EXT_ACCESS_PRE_AUTH_DURATION_SEC").CallAsync<ulong>().Result;

            // Get the contract for the Pool by specifying the pool address
            contract = AppServices.web3.Eth.GetContract(AppModelConfig.SETUPI.abi, adr.PoolContractAdr);            
            setup.POOL_NAME = contract.GetFunction("POOL_NAME").CallAsync<string>().Result;

            // Constants used by the Insurance Pool
            setup.WC_POOL_TARGET_TIME_SEC = contract.GetFunction("WC_POOL_TARGET_TIME_SEC").CallAsync<ulong>().Result;
            setup.DURATION_TO_BOND_MATURITY_SEC = contract.GetFunction("DURATION_TO_BOND_MATURITY_SEC").CallAsync<ulong>().Result;
            setup.DURATION_BOND_LOCK_NEXT_STATE_SEC = contract.GetFunction("DURATION_BOND_LOCK_NEXT_STATE_SEC").CallAsync<ulong>().Result;
            setup.DURATION_WC_EXPENSE_HISTORY_DAYS = contract.GetFunction("DURATION_WC_EXPENSE_HISTORY_DAYS").CallAsync<ulong>().Result;

            // Yield constants
            setup.YAC_PER_INTERVAL_PPB = contract.GetFunction("YAC_PER_INTERVAL_PPB").CallAsync<ulong>().Result;
            setup.YAC_INTERVAL_DURATION_SEC = contract.GetFunction("YAC_INTERVAL_DURATION_SEC").CallAsync<ulong>().Result;
            setup.YAC_EXPENSE_THRESHOLD_PPT = contract.GetFunction("YAC_EXPENSE_THRESHOLD_PPT").CallAsync<ulong>().Result;
            setup.MIN_YIELD_PPB = contract.GetFunction("MIN_YIELD_PPB").CallAsync<ulong>().Result;
            setup.MAX_YIELD_PPB = contract.GetFunction("MAX_YIELD_PPB").CallAsync<ulong>().Result;

            // Bond constants
            setup.MIN_BOND_PRINCIPAL_CU = contract.GetFunction("MIN_BOND_PRINCIPAL_CU").CallAsync<ulong>().Result;
            setup.MAX_BOND_PRINCIPAL_CU = contract.GetFunction("MAX_BOND_PRINCIPAL_CU").CallAsync<ulong>().Result;
            setup.BOND_REQUIRED_SECURITY_REFERENCE_PPT = contract.GetFunction("BOND_REQUIRED_SECURITY_REFERENCE_PPT").CallAsync<ulong>().Result;

            // Policy constants
            setup.MIN_POLICY_CREDIT_CU = contract.GetFunction("MIN_POLICY_CREDIT_CU").CallAsync<ulong>().Result;
            setup.MAX_POLICY_CREDIT_CU = contract.GetFunction("MAX_POLICY_CREDIT_CU").CallAsync<ulong>().Result;
            setup.MAX_DURATION_POLICY_RECONCILIATION_DAYS = contract.GetFunction("MAX_DURATION_POLICY_RECONCILIATION_DAYS").CallAsync<ulong>().Result;
            setup.POLICY_RECONCILIATION_SAFETY_MARGIN = contract.GetFunction("POLICY_RECONCILIATION_SAFETY_MARGIN").CallAsync<ulong>().Result;
            setup.MIN_DURATION_POLICY_PAUSED_DAY = contract.GetFunction("MIN_DURATION_POLICY_PAUSED_DAY").CallAsync<ulong>().Result;
            setup.MAX_DURATION_POLICY_PAUSED_DAY = contract.GetFunction("MAX_DURATION_POLICY_PAUSED_DAY").CallAsync<ulong>().Result;
            setup.DURATION_POLICY_POST_LAPSED_DAY = contract.GetFunction("DURATION_POLICY_POST_LAPSED_DAY").CallAsync<ulong>().Result;
            setup.MAX_DURATION_POLICY_LAPSED_DAY = contract.GetFunction("MAX_DURATION_POLICY_LAPSED_DAY").CallAsync<ulong>().Result;

            // Pool processing costants
            setup.POOL_DAILY_PROCESSING_OFFSET_SEC = contract.GetFunction("POOL_DAILY_PROCESSING_OFFSET_SEC").CallAsync<ulong>().Result;
            setup.POOL_DAYLIGHT_SAVING_ADJUSTMENT_SEC = contract.GetFunction("POOL_DAYLIGHT_SAVING_ADJUSTMENT_SEC").CallAsync<ulong>().Result;
            setup.POOL_TIME_ZONE_OFFSET = contract.GetFunction("POOL_TIME_ZONE_OFFSET").CallAsync<long>().Result;

            // Operator and Trust fees
            setup.POOL_OPERATOR_FEE_PPT = contract.GetFunction("POOL_OPERATOR_FEE_PPT").CallAsync<ulong>().Result;
            setup.TRUST_FEE_PPT = contract.GetFunction("TRUST_FEE_PPT").CallAsync<ulong>().Result;

            // Hashes of the bank account owner and bank account number (sha3(accountOwner, accountNumber))
            setup.PREMIUM_ACCOUNT_PAYMENT_HASH = AppModelConfig.convertToHex(contract.GetFunction("PREMIUM_ACCOUNT_PAYMENT_HASH").CallAsync<string>().Result);
            setup.BOND_ACCOUNT_PAYMENT_HASH = AppModelConfig.convertToHex(contract.GetFunction("BOND_ACCOUNT_PAYMENT_HASH").CallAsync<string>().Result);
            setup.FUNDING_ACCOUNT_PAYMENT_HASH = AppModelConfig.convertToHex(contract.GetFunction("FUNDING_ACCOUNT_PAYMENT_HASH").CallAsync<string>().Result);
            setup.TRUST_ACCOUNT_PAYMENT_HASH = AppModelConfig.convertToHex(contract.GetFunction("TRUST_ACCOUNT_PAYMENT_HASH").CallAsync<string>().Result);
            setup.OPERATOR_ACCOUNT_PAYMENT_HASH = AppModelConfig.convertToHex(contract.GetFunction("OPERATOR_ACCOUNT_PAYMENT_HASH").CallAsync<string>().Result);
            setup.SETTLEMENT_ACCOUNT_PAYMENT_HASH = AppModelConfig.convertToHex(contract.GetFunction("SETTLEMENT_ACCOUNT_PAYMENT_HASH").CallAsync<string>().Result);
            setup.ADJUSTOR_ACCOUNT_PAYMENT_HASH = AppModelConfig.convertToHex(contract.GetFunction("ADJUSTOR_ACCOUNT_PAYMENT_HASH").CallAsync<string>().Result);
            
            return setup;
        }

        public object Get(GetEcosystemContractAddresses request) {
            return AppServices.GetEcosystemAdr(request.ContractAdr);
        }

        // ********************************************************************************************
        // *** TRUST
        // ********************************************************************************************

        public object Get(GetTrustLogs request) {
            // Retrieve the block parameters
            (BlockParameter fromBlock, BlockParameter toBlock) = AppServices.getBlockParameterConfiguration(request.FromBlock, request.ToBlock, 
                (request.Subject.IsEmpty() == true) && (request.Address.IsEmpty() == true) && (request.Info.IsEmpty() == true));

            // Create the filter variables for selecting only the requested log entries
            object[] ft1 = (request.Subject.IsEmpty() == true ? null : new object[]{ AppModelConfig.convertToHex64(request.Subject).HexToByteArray() });
            object[] ft2 = (request.Address.IsEmpty() == true ? null : new object[]{ request.Address });
            object[] ft3 = (request.Info.IsEmpty() == true ? null : new object[]{ AppModelConfig.convertToHex64(request.Info).HexToByteArray() });

            // Retrieve the contract info
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.TRUST.abi, AppServices.GetEcosystemAdr(request.ContractAdr).TrustContractAdr);
            
            // Create the filter input to extract the requested log entries
            var filterInput = contract.GetEvent("LogTrust").CreateFilterInput(filterTopic1: ft1, filterTopic2: ft2, filterTopic3: ft3, fromBlock: fromBlock, toBlock: toBlock);
            
            // Extract all the logs as specified by the filter input
            var res = AppServices.web3.Eth.Filters.GetLogs.SendRequestAsync(filterInput).Result;

            // Create the return instance
            var logs = new TrustLogs() { EventLogs = new List<TrustEventLog>() };

            // Interate through all the returned logs and add them to the logs list
            for (int i=res.Length - 1; i>=0; i--) {
                logs.EventLogs.Add(new TrustEventLog() {
                    BlockNumber = Convert.ToUInt64(res[i].BlockNumber.HexValue, 16),
                    Subject = AppModelConfig.FromHexString(res[i].Topics[1].ToString()),            
                    Address = AppModelConfig.getAdrFromString32(res[i].Topics[2].ToString()),
                    Info = res[i].Topics[3].ToString().Replace(AppModelConfig.EMPTY_HASH, ""),
                    Timestamp = Convert.ToUInt64(res[i].Data.Substring(2 + 0 * 64, 64), 16)
                });
            }

            // Return the list of bond logs
            return logs;
        }

        // ********************************************************************************************
        // *** ECOSYSTEM TRANSACTIONS - Set Working Capital expenses, Adjust daylight saving, etc.
        // ********************************************************************************************

        public object Put(SetWcExpenses request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.TRUST.abi, 
                AppServices.GetEcosystemAdr(request.ContractAdr).TrustContractAdr,
                request.SigningPrivateKey,
                "setWcExpenses",
                request.Amount
            );
        }

        public object Put(AdjustDaylightSaving request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.TRUST.abi, 
                AppServices.GetEcosystemAdr(request.ContractAdr).TrustContractAdr,
                request.SigningPrivateKey,
                "adjustDaylightSaving"
            );
        }

        // ********************************************************************************************
        // *** HOSTING ENVIRONMENT AND CONFIGURATION SETUP
        // ********************************************************************************************

        public object Get(HostingEnvironmentSetup request) {
            // Returns the configured hosting environment details
            return new HostingEnvironment {
                ApiServiceHostingEnvironmentName = AppModelConfig.hostingEnvironment.EnvironmentName,
                MaxWaitDurationForTransactionReceipt = AppModelConfig.maxWaitDurationForTransactionReceipt,
                DefaultNumberEntriesForLazyLoading = AppModelConfig.defaultNumberEntriesForLazyLoading,
                DefaultBlockRangeForEventLogLoading = AppModelConfig.defaultBlockRangeForEventLogLoading,
                Web3UrlEndpoint = AppModelConfig.WEB3_URL_ENDPOINT,
                AutoSchedulePingDuration = AppServices.getPingTimerInterval(),
                ExternalAccessInterfaceAbi = AppModelConfig.EXTACCESSI.abi,
                InternalAccessInterfaceAbi = AppModelConfig.INTACCESSI.abi,
                SetupInterfaceAbi = AppModelConfig.SETUPI.abi,
                LibraryAbi = AppModelConfig.LIB.abi,
                PoolAbi = AppModelConfig.POOL.abi,
                BondAbi = AppModelConfig.BOND.abi,
                BankAbi = AppModelConfig.BANK.abi,
                PolicyAbi = AppModelConfig.POLICY.abi,
                SettlementAbi = AppModelConfig.SETTLEMENT.abi,
                AdjustorAbi = AppModelConfig.ADJUSTOR.abi,
                TimerAbi = AppModelConfig.TIMER.abi,
                TrustAbi = AppModelConfig.TRUST.abi
            };
        }

        // ********************************************************************************************
        // *** EXTERNAL ACCESS KEYS - AuthKeys management
        // ********************************************************************************************

        public object Get(GetContractAuthKeys request) {
            // Get the contract for the Pool by specifying the pool address
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.EXTACCESSI.abi, request.ContractAdr);
            // Create a new return instance for Pool Metadata and set the Pool contract address
            ContractAuthKeys keys = contract.GetFunction("getExtAccessKey").CallDeserializingToObjectAsync<ContractAuthKeys>().Result;
            keys.PreAuthKeyUsed = contract.GetFunction("getPreAuthKey").CallAsync<string>().Result;
            keys.PreAuthExpiry = contract.GetFunction("getPreAuthExpiry").CallAsync<ulong>().Result;
            // Return the metadata available on this bond contract
            return keys;
        }

        public object Put(PreAuth request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.EXTACCESSI.abi, 
                request.ContractAdr, 
                request.SigningPrivateKey,
                "preAuth",
                null
            );
        }

        public object Put(AddAuthKey request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.EXTACCESSI.abi, 
                request.ContractAdr, 
                request.SigningPrivateKey,
                "addKey",
                request.KeyToAddAdr
            );
        }

        public object Put(RotateAuthKey request) {
            // Submit and return the transaction hash of the broadcasted transaction
            return AppServices.createSignPublishTransaction(
                AppModelConfig.EXTACCESSI.abi, 
                request.ContractAdr, 
                request.SigningPrivateKey,
                "rotateKey",
                null
            );
        }
    }
}