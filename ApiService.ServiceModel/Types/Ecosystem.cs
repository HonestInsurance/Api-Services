/**
 * @description Ecosystem Data Transfer Objects (DTOs)
 * @copyright (c) 2017 HIC Limited (NZBN: 9429043400973)
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using ServiceStack;
using System;
using System.Collections.Generic;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.ABI.FunctionEncoding.Attributes;


namespace ApiService.ServiceModel
{
    // ********************************************************************************************
    // *** ECOSYSTEM
    // ********************************************************************************************
    
    [FunctionOutput]
    public class EcosystemStatus {
        // Day of the Pool, Adjustment variables for Daylight saving and Leap seconds
        [ApiMember(IsRequired = true, Description = "The day the pool is currently in (days since 01/01/1970")]
        public ulong currentPoolDay { get; set; }
        [ApiMember(IsRequired = true, Description = "Indicates if the pool is currently in Summer or Winter time zone")]
        public bool  isWinterTime { get; set; }
        [ApiMember(IsRequired = true, Description = "Indicates if Summer/Winter daylight saving adjustment is scheduled for next overnight processing")]
        public bool daylightSavingScheduled { get; set; }

        // Bank account balances
        public ulong WC_Bal_FA_Cu { get; set; }
        public ulong WC_Bal_BA_Cu { get; set; }
        public ulong WC_Bal_PA_Cu { get; set; }

        // Variables used by the Insurance Pool during overnight processing only
        public bool  overwriteWcExpenses { get; set; }
        public ulong WC_Exp_Cu { get; set; }
        
        // Pool variables as defined in the model
        public ulong WC_Locked_Cu { get; set; }
        public ulong WC_Bond_Cu { get; set; }
        public ulong WC_Transit_Cu { get; set; }
        public ulong B_Yield_Ppb { get; set; }
        public ulong B_Gradient_Ppq { get; set; }

        // Flag to indicate if the bond yield accelleration is operational (and scheduled by the timer)
        public bool bondYieldAccellerationScheduled { get; set; }
        public ulong bondYieldAccelerationThreshold { get; set; }

        [ApiMember(IsRequired = true, Description = "Total number of active policy risk points that are currently insured by the pool")]
        public ulong totalIssuedPolicyRiskPoints { get; set; }

        [ApiMember(IsRequired = true, Description = "The total amount of funds that were spent by the insurnace pool today")]
        public ulong fundingAccountPaymentsTracking_Cu { get; set; }

        [ApiMember(IsRequired = true, Description = "The timestamp the ping execution was executed the last time")]
        public ulong lastPingExececution { get; set; }

        [ApiMember(IsRequired = true, Description = "Info on the bonds")]
        public ListInfo BondListInfo  { get; set; }
        [ApiMember(IsRequired = true, Description = "Info on the policies")]
        public ListInfo PolicyListInfo  { get; set; }
        [ApiMember(IsRequired = true, Description = "Info on the adjustors")]
        public ListInfo AdjustorListInfo  { get; set; }
        [ApiMember(IsRequired = true, Description = "Info on the settlements")]
        public ListInfo SettlementListInfo  { get; set; }
    }

    public class EcosystemLogs {
        [ApiMember(IsRequired = true, Description = "Ecosystem log entries")]
        public List<EcosystemLog> Logs {get; set;}
    }

    public class EcosystemLog : IParseLog {

        public void parseLog(FilterLog fl) {
            BlockNumber = Convert.ToUInt64(fl.BlockNumber.HexValue, 16);
            Subject = AppModelConfig.FromHexString(fl.Topics[1].ToString());          
            Day = Convert.ToUInt64(fl.Topics[2].ToString(), 16);
            Value = Convert.ToUInt64(fl.Topics[3].ToString(), 16);
            Timestamp = Convert.ToUInt64(fl.Data.Substring(2 + 0 * 64, 64), 16);
        }

        [ApiMember(IsRequired = true, Description = "The block number this event was triggered")]
        public ulong BlockNumber { get; set; }
        
        [ApiMember(IsRequired = true, Description = "The subject of the Pool event log entry")]
        public string Subject { get; set; }

        [ApiMember(IsRequired = true, Description = "The day (days since 1/1/1970) when on this event logged")]
        public ulong Day { get; set; }

        [ApiMember(IsRequired = true, Description = "The value describing the event outcome.")]
        public ulong Value { get; set; }

        [ApiMember(IsRequired = true, Description = "The timestamp this event was triggered")]
        public ulong Timestamp { get; set; }
    }

    [FunctionOutput]
    public class EcosystemConfiguration {
        [ApiMember(IsRequired = true, Description = "The name of this insurance pool")]
        public string POOL_NAME { get; set; }

        // Constants used by the Insurance Pool
        public ulong WC_POOL_TARGET_TIME_SEC { get; set; }
        public ulong DURATION_TO_BOND_MATURITY_SEC { get; set; }
        public ulong DURATION_BOND_LOCK_NEXT_STATE_SEC { get; set; }
        public ulong DURATION_WC_EXPENSE_HISTORY_DAYS { get; set; }

        // Yield constants
        public ulong YAC_PER_INTERVAL_PPB { get; set; }
        public ulong YAC_INTERVAL_DURATION_SEC { get; set; }
        public ulong YAC_EXPENSE_THRESHOLD_PPT { get; set; }
        public ulong MIN_YIELD_PPB { get; set; }
        public ulong MAX_YIELD_PPB { get; set; }

        // Bond constants
        public ulong MIN_BOND_PRINCIPAL_CU { get; set; }
        public ulong MAX_BOND_PRINCIPAL_CU { get; set; }
        public ulong BOND_REQUIRED_SECURITY_REFERENCE_PPT { get; set; }

        // Policy constants
        public ulong MIN_POLICY_CREDIT_CU { get; set; }
        public ulong MAX_POLICY_CREDIT_CU { get; set; }
        public ulong MAX_DURATION_POLICY_RECONCILIATION_DAYS { get; set; }
        public ulong POLICY_RECONCILIATION_SAFETY_MARGIN { get; set; }
        public ulong MIN_DURATION_POLICY_PAUSED_DAY { get; set; }
        public ulong MAX_DURATION_POLICY_PAUSED_DAY { get; set; }
        public ulong DURATION_POLICY_POST_LAPSED_DAY { get; set; }
        public ulong MAX_DURATION_POLICY_LAPSED_DAY { get; set; }

        // Pool processing costants
        public ulong POOL_DAILY_PROCESSING_OFFSET_SEC { get; set; }
        public ulong POOL_DAYLIGHT_SAVING_ADJUSTMENT_SEC { get; set; }
        public long POOL_TIME_ZONE_OFFSET { get; set; }

        // Operator and Trust fees
        public ulong POOL_OPERATOR_FEE_PPT { get; set; }
        public ulong TRUST_FEE_PPT { get; set; }

        // Pre-authorisation duration for external authentication
        public ulong EXT_ACCESS_PRE_AUTH_DURATION_SEC { get; set; }

        // Hashes of the bank account owner and bank account number (sha3(accountOwner, accountNumber))
        public string PREMIUM_ACCOUNT_PAYMENT_HASH { get; set; }
        public string BOND_ACCOUNT_PAYMENT_HASH { get; set; }
        public string FUNDING_ACCOUNT_PAYMENT_HASH { get; set; }
        public string TRUST_ACCOUNT_PAYMENT_HASH { get; set; }
        public string OPERATOR_ACCOUNT_PAYMENT_HASH { get; set; }
        public string SETTLEMENT_ACCOUNT_PAYMENT_HASH { get; set; }
        public string ADJUSTOR_ACCOUNT_PAYMENT_HASH { get; set; }
    }

    [FunctionOutput]
    public class EcosystemContractAddresses {
        [ApiMember(IsRequired = true, Description = "Configured trust contract address")]
        [Parameter("address", "trustContractAdr", 1)]
        public string TrustContractAdr { get; set; }

        [ApiMember(IsRequired = true, Description = "Configured pool contract address")]
        [Parameter("address", "poolContractAdr", 2)]
        public string PoolContractAdr { get; set; }

        [ApiMember(IsRequired = true, Description = "Configured bond contract address")]
        [Parameter("address", "bondContractAdr", 3)]
        public string BondContractAdr { get; set; }

        [ApiMember(IsRequired = true, Description = "Configured bank contract address")]
        [Parameter("address", "bankContractAdr", 4)]
        public string BankContractAdr { get; set; }

        [ApiMember(IsRequired = true, Description = "Configured policy contract address")]
        [Parameter("address", "policyContractAdr", 5)]
        public string PolicyContractAdr { get; set; }

        [ApiMember(IsRequired = true, Description = "Configured claim contract address")]
        [Parameter("address", "settlementContractAdr", 6)]
        public string SettlementContractAdr { get; set; }

        [ApiMember(IsRequired = true, Description = "Configured adjustor contract address")]
        [Parameter("address", "adjustorContractAdr", 7)]
        public string AdjustorContractAdr { get; set; }

        [ApiMember(IsRequired = true, Description = "Configured timer contract address")]
        [Parameter("address", "timerContractAdr", 8)]
        public string TimerContractAdr { get; set; }
    }

    [FunctionOutput]
    public class ListInfo {
        [ApiMember(IsRequired = true, Description = "The starting index of the potentially active list items (all previous list entries are archived)")]
        [Parameter("uint", "firstIdx", 1)]
        public ulong ActiveItemsListStartIdx { get; set; }

        [Parameter("uint", "nextIdx", 2)]
        private ulong CountAllItemsInit { set { CountAllItems = value - 1; } }
        [ApiMember(IsRequired = true, Description = "Total number of all entries that have been added to this list to date")]
        public ulong CountAllItems { get; set; }

        [ApiMember(IsRequired = true, Description = "Total number of active entries in the list")]
        [Parameter("uint", "count", 3)]
        public ulong CountActiveItems { get; set; }
    }

    // ********************************************************************************************
    // *** TRUST
    // ********************************************************************************************

    public class TrustLogs {
        [ApiMember(IsRequired = true, Description = "Trust log entries")]
        public List<TrustLog> Logs {get; set;}
    }

    public class TrustLog : IParseLog {

        public void parseLog(FilterLog fl) {
            BlockNumber = Convert.ToUInt64(fl.BlockNumber.HexValue, 16);
            Subject = AppModelConfig.FromHexString(fl.Topics[1].ToString());          
            Address = AppModelConfig.getAdrFromString32(fl.Topics[2].ToString());
            Info = fl.Topics[3].ToString().Replace(AppModelConfig.EMPTY_HASH, "");
            Timestamp = Convert.ToUInt64(fl.Data.Substring(2 + 0 * 64, 64), 16);
        }

        [ApiMember(IsRequired = true, Description = "The block number this event was triggered")]
        public ulong BlockNumber { get; set; }
        
        [ApiMember(IsRequired = true, Description = "The subject of the Pool event log entry")]
        public string Subject { get; set; }

        [ApiMember(IsRequired = true, Description = "The address info on this log entry")]
        public string Address { get; set; }

        [ApiMember(IsRequired = true, Description = "Further info describing the event outcome")]
        public string Info { get; set; }

        [ApiMember(IsRequired = true, Description = "The timestamp this event was triggered")]
        public ulong Timestamp { get; set; }
    }

    // ********************************************************************************************
    // *** HOSTING ENVIRONMENT AND CONFIGURATION SETUP
    // ********************************************************************************************

    [FunctionOutput]
    public class HostingEnvironment {
        [ApiMember(IsRequired = true, Description = "Name of the hostig environment this API service is running on")]
        public string ApiServiceHostingEnvironmentName { get; set; }
        
        [ApiMember(IsRequired = true, Description = "The default gas price to be used for new transactions to be published")]
        public ulong DefaultGasPrice { get; set; }
        [ApiMember(IsRequired = true, Description = "The default gas limit to be used for new transactions to be published")]
        public ulong DefaultGasLimit { get; set; }
        
        [ApiMember(IsRequired = true, Description = "The maximum duration the API service is awaitig a transaction receipt before timing out")]
        public ulong MaxWaitDurationForTransactionReceipt { get; set; }
        [ApiMember(IsRequired = true, Description = "Default number of list records returned required for lazy loading")]
        public ulong DefaultNumberEntriesForLazyLoading { get; set; }
        [ApiMember(IsRequired = true, Description = "Default range of Blockchain blocks that are search for retrieving log files required for lazy loading")]
        public ulong DefaultBlockRangeForEventLogLoading { get; set; }
        
        [ApiMember(IsRequired = true, Description = "The endpoint address of the Blockchain client this API service is connected to")]
        public string Web3UrlEndpoint { get; set; }
        [ApiMember(IsRequired = true, Description = "Indicates if (and in what time intervall) the api service creates automatic ping transactions")]
        public ulong AutoSchedulePingDuration { get; set; }
        
        [ApiMember(IsRequired = true, Description = "ABI of the External Access interface")]
        public string ExternalAccessInterfaceAbi { get; set; }
        [ApiMember(IsRequired = true, Description = "ABI of the Internal Access interface")]
        public string InternalAccessInterfaceAbi { get; set; }
        [ApiMember(IsRequired = true, Description = "ABI of the Setup interface")]
        public string SetupInterfaceAbi { get; set; }
        [ApiMember(IsRequired = true, Description = "ABI of the Library contract")]
        public string LibraryAbi { get; set; }
        [ApiMember(IsRequired = true, Description = "ABI of the Pool contract")]
        public string PoolAbi { get; set; }
        [ApiMember(IsRequired = true, Description = "ABI of the Bond contract")]
        public string BondAbi { get; set; }
        [ApiMember(IsRequired = true, Description = "ABI of the Bank contract")]
        public string BankAbi { get; set; }
        [ApiMember(IsRequired = true, Description = "ABI of the Policy contract")]
        public string PolicyAbi { get; set; }
        [ApiMember(IsRequired = true, Description = "ABI of the Settlement contract")]
        public string SettlementAbi { get; set; }
        [ApiMember(IsRequired = true, Description = "ABI of the Adjustor contract")]
        public string AdjustorAbi { get; set; }
        [ApiMember(IsRequired = true, Description = "ABI of the Timer contract")]
        public string TimerAbi { get; set; }
        [ApiMember(IsRequired = true, Description = "ABI of the Trust contract")]
        public string TrustAbi { get; set; }
    }

    // ********************************************************************************************
    // *** EXTERNAL ACCESS KEYS - AuthKeys management
    // ********************************************************************************************

    [FunctionOutput]
    public class ContractAuthKeys {
        [ApiMember(IsRequired = true, Description = "Authorisation key 0 of the contract")]
        [Parameter("address", "authKey0", 1)]
        public string AuthKey0 { get; set; }

        [ApiMember(IsRequired = true, Description = "Authorisation key 1 of the contract")]
        [Parameter("address", "authKey1", 2)]
        public string AuthKey1 { get; set; }

        [ApiMember(IsRequired = true, Description = "Authorisation key 2 of the contract")]
        [Parameter("address", "authKey2", 3)]
        public string AuthKey2 { get; set; }

        [ApiMember(IsRequired = true, Description = "Authorisation key 3 of the contract")]
        [Parameter("address", "authKey3", 4)]
        public string AuthKey3 { get; set; }

        [ApiMember(IsRequired = true, Description = "Authorisation key 4 of the contract")]
        [Parameter("address", "authKey4", 5)]
        public string AuthKey4 { get; set; }

        [ApiMember(IsRequired = true, Description = "Pre authorisation key used")]
        public string PreAuthKeyUsed { get; set; }
        
        [ApiMember(IsRequired = true, Description = "The timestamp when pre-authorisation expires")]
        public ulong PreAuthExpiry { get; set; }
    }
}
