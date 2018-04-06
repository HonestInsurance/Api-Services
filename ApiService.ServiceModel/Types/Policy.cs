/**
 * @description Policy Data Transfer Objects (DTOs)
 * @copyright (c) 2017 Honest Insurance
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using ServiceStack;
using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;


namespace ApiService.ServiceModel
{
    [FunctionOutput]
    public class PolicyList {
        [ApiMember(IsRequired = true, Description = "Details on all the items in this list")]
        public ListInfo Info { get; set; }

        [ApiMember(IsRequired = true, Description = "List of the items")]
        public List<PolicyDetail> Items { get; set;}
    }

    [FunctionOutput]
    public class PolicyDetail {
        [ApiMember(IsRequired = true, Description = "The hash of the policy")]
        public string Hash { get; set; }

        [ApiMember(IsRequired = true, Description = "The unique index of the policy")]
        [Parameter("uint", "idx", 1)]
        public ulong Idx { get; set; }

        [ApiMember(IsRequired = true, Description = "Address of the policy's owner")]
        [Parameter("address", "owner", 2)]
        public string Owner { get; set; }

        [Parameter("bytes32", "paymentAccountHash", 3)]
        public byte[] PaymentAccountHashInit { set { PaymentAccountHash = AppModelConfig.convertToHex(value); } }
        [ApiMember(IsRequired = false, Description = "Bank payment hash as provided by the bank")]
        public string PaymentAccountHash { get; set; }

        [Parameter("bytes32", "documentHash", 4)]
        public byte[] DocumentHashInit { set { DocumentHash = AppModelConfig.convertToHex(value); } }
        [ApiMember(IsRequired = false, Description = "Hash of the policy document")]
        public string DocumentHash { get; set; }

        [ApiMember(IsRequired = true, Description = "The policy risk points")]
        [Parameter("uint", "riskPoints", 5)]
        public ulong RiskPoints { get; set; }

        [ApiMember(IsRequired = true, Description = "Total amount of funds that have been credited towards this policy by the policy owner")]
        [Parameter("uint", "premiumCredited_Cu", 6)]
        public ulong PremiumCredited { get; set; }
        
        [ApiMember(IsRequired = true, Description = "The amount this policy has been charged in premiums so far. This value is denominated in ppt => divide value by 1000!")]
        [Parameter("uint", "premiumCharged_Cu_Ppt", 7)]
        public ulong PremiumCharged { get; set; }

        [Parameter("uint", "state", 8)]
        public ulong StateInit { set { State = (PolicyState)value; } }
        [ApiMember(IsRequired = true, Description = "State of the policy")]
        public PolicyState State { get; set; }

        [ApiMember(IsRequired = true, Description = "Date last policy reconciliation was performed")]
        [Parameter("uint", "lastReconciliationDay", 9)]
        public ulong LastReconciliationDay { get; set; }

        [ApiMember(IsRequired = true, Description = "Day to perform next policy reconciliation")]
        [Parameter("uint", "nextReconciliationDay", 10)]
        public ulong NextReconciliationDay { get; set; }

        [ApiMember(IsRequired = true, Description = "Log entries for this policy")]
        public List<PolicyEventLog> EventLogs {get; set;}
    }

    [FunctionOutput]
    public class PolicyLogs {
        [ApiMember(IsRequired = true, Description = "Policy log entries")]
        public List<PolicyEventLog> EventLogs { get; set; }
    }

    [FunctionOutput]
    public class PolicyEventLog {
        [ApiMember(IsRequired = true, Description = "The block number this event was triggered")]
        public ulong BlockNumber { get; set; }

        [ApiMember(IsRequired = true, Description = "The hash of the policy")]
        public string Hash { get; set; }

        [ApiMember(IsRequired = true, Description = "The owner of the policy entry")]
        public string Owner { get; set; }

        [ApiMember(IsRequired = true, Description = "Further information describing the event outcome.")]
        public string Info { get; set; }
        
        [ApiMember(IsRequired = true, Description = "The timestamp this event was triggered")]
        public ulong Timestamp { get; set; }

        [ApiMember(IsRequired = true, Description = "The new state of the Policy")]
        public PolicyState State { get; set; }
    }

    public enum PolicyState {
        /*0*/ Paused,       // Policy owner deactivated the policy termporarily (temporarily prevent from continuing or being in force or effect)
        /*1*/ Issued,       // Policy is in an active state
        /*2*/ Lapsed,       // Policy ran out of funds (no longer valid; expired.)
        /*3*/ PostLapsed,   // Policy has been refunded and is due for Re-Issuing
        /*4*/ Retired       // The policy has been cancelled and is archived permanently.
    }
}