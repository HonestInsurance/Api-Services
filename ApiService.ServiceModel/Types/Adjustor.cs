/**
 * @description Adjustor Data Transfer Objects (DTOs)
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
    public class AdjustorList {
        [ApiMember(IsRequired = true, Description = "Details on all the items in this list")]
        public ListInfo Info { get; set; }

        [ApiMember(IsRequired = true, Description = "List of the items")]
        public List<AdjustorDetail> Items { get; set;}
    }

    [FunctionOutput]
    public class AdjustorDetail {
        [ApiMember(IsRequired = true, Description = "The hash of the adjustor")]
        public string Hash { get; set; }

        [ApiMember(IsRequired = true, Description = "The unique index of the adjustor")]
        [Parameter("uint", "idx", 1)]
        public ulong Idx { get; set; }

        [ApiMember(IsRequired = true, Description = "Address of the adjustor's owner")]
        [Parameter("address", "owner", 2)]
        public string Owner { get; set; }

        [ApiMember(IsRequired = true, Description = "The threshold amount this adjustor is authorised to approve settlements")]
        [Parameter("uint", "settlementApprovalAmount_Cu", 3)]
        public ulong SettlementApprovalAmount { get; set; }

        [ApiMember(IsRequired = true, Description = "The upper limit this adjustor is authorised to set policie's risk points")]
        [Parameter("uint", "policyRiskPointLimit", 4)]
        public ulong PolicyRiskPointLimit { get; set; }

        [Parameter("bytes32", "serviceAgreementHash", 5)]
        public byte[] ServiceAgreementHashInit { set { ServiceAgreementHash = AppModelConfig.convertToHex(value); } }
        [ApiMember(IsRequired = false, Description = "Hash adjustor's service agreement document with the ecosystem")]
        public string ServiceAgreementHash { get; set; }

        [ApiMember(IsRequired = true, Description = "Log entries for this adjustor")]
        public List<AdjustorLog> Logs {get; set;}
    }

    public class AdjustorLogs {
        [ApiMember(IsRequired = true, Description = "Log entries")]
        public List<AdjustorLog> Logs { get; set; }
    }

    public class AdjustorLog {

        public AdjustorLog(FilterLog fl){
            BlockNumber = Convert.ToUInt64(fl.BlockNumber.HexValue, 16);
            Hash = fl.Topics[1].ToString();
            Owner = AppModelConfig.getAdrFromString32(fl.Topics[2].ToString());
            Timestamp = Convert.ToUInt64(fl.Data.Substring(2 + 0 * 64, 64), 16);
            if (AppModelConfig.isEmptyHash(fl.Topics[3].ToString()) == true)
                Info = "";
            else if (fl.Topics[3].ToString().StartsWith("0x000000") == true)
                Info = Convert.ToInt64(fl.Topics[3].ToString(), 16).ToString();
            else Info = fl.Topics[3].ToString();
        }

        [ApiMember(IsRequired = true, Description = "The block number this event was triggered")]
        public ulong BlockNumber { get; set; }

        [ApiMember(IsRequired = true, Description = "The hash of the adjustor")]
        public string Hash { get; set; }

        [ApiMember(IsRequired = true, Description = "The owner of the adjustor entry")]
        public string Owner { get; set; }

        [ApiMember(IsRequired = true, Description = "Further information describing the event outcome.")]
        public string Info { get; set; }
        
        [ApiMember(IsRequired = true, Description = "The timestamp this event was triggered")]
        public ulong Timestamp { get; set; }
    }
}