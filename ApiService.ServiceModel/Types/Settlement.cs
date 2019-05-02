/**
 * @description Settlement Data Transfer Objects (DTOs)
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
    [FunctionOutput]
    public class SettlementList {
        [ApiMember(IsRequired = true, Description = "Details on all the items in this list")]
        public ListInfo Info { get; set; }

        [ApiMember(IsRequired = true, Description = "List of the items")]
        public List<SettlementDetail> Items { get; set;}
    }

    [FunctionOutput]
    public class SettlementDetail {
        [ApiMember(IsRequired = true, Description = "The hash of the settlement")]
        public string Hash { get; set; }

        [ApiMember(IsRequired = true, Description = "The unique index of the settlement")]
        [Parameter("uint", "idx", 1)]
        public ulong Idx { get; set; }

        [ApiMember(IsRequired = true, Description = "The final OR anticipated settlement amount")]
        [Parameter("uint", "settlementAmount", 2)]
        public ulong SettlementAmount { get; set; }

        [Parameter("uint", "state", 3)]
        public ulong StateInit { set { State = (SettlementState)value; } }
        [ApiMember(IsRequired = true, Description = "State of the settlement")]
        public SettlementState State { get; set; }

        [ApiMember(IsRequired = true, Description = "Log entries for this settlement")]
        public List<SettlementLog> EventLogs {get; set;}
    }

    public class SettlementLogs {
        [ApiMember(IsRequired = true, Description = "Settlement log entries")]
        public List<SettlementLog> Logs { get; set; }
    }

    public class SettlementLog : IParseLog {

        public void parseLog(FilterLog fl) {
            BlockNumber = Convert.ToUInt64(fl.BlockNumber.HexValue, 16);
            SettlementHash = fl.Topics[1].ToString();
            AdjustorHash = fl.Topics[2].ToString();
            Info = fl.Topics[3].ToString();
            Timestamp = Convert.ToUInt64(fl.Data.Substring(2 + 0 * 64, 64), 16);
            State = (SettlementState)Convert.ToInt32(fl.Data.Substring(2 + 1 * 64,64), 16);
        }

        [ApiMember(IsRequired = true, Description = "The block number this event was triggered")]
        public ulong BlockNumber { get; set; }

        [ApiMember(IsRequired = true, Description = "The hash of the settlement")]
        public string SettlementHash { get; set; }

        [ApiMember(IsRequired = true, Description = "The hash of the adjustor")]
        public string AdjustorHash { get; set; }

        [ApiMember(IsRequired = true, Description = "Further information describing the event outcome.")]
        public string Info { get; set; }
        
        [ApiMember(IsRequired = true, Description = "The timestamp this event was triggered")]
        public ulong Timestamp { get; set; }

        [ApiMember(IsRequired = true, Description = "The new state of the Policy")]
        public SettlementState State { get; set; }
    }

    public enum SettlementState {
        /*0*/ Created,      // An adjustor created a new Settlement
        /*1*/ Processing,   // Settlement is in a processing state
        /*2*/ Settled       // The settlement request has been completed (no further amendments are possible)
    }
}