/**
 * @description Bond Data Transfer Objects (DTOs)
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
    public class BondList {
        [ApiMember(IsRequired = true, Description = "Details on all the items in this list")]
        public ListInfo Info { get; set; }

        [ApiMember(IsRequired = true, Description = "List of the items")]
        public List<BondDetail> Items { get; set;}
    }

    [FunctionOutput]
    public class BondDetail {
        [ApiMember(IsRequired = true, Description = "The hash of the bond")]
        public string Hash { get; set; }

        [ApiMember(IsRequired = true, Description = "The unique index of the bond")]
        [Parameter("uint", "idx", 1)]
        public ulong Idx { get; set; }

        [ApiMember(IsRequired = true, Description = "Address of the bond's owner")]
        [Parameter("address", "owner", 2)]
        public string Owner { get; set; }

        [Parameter("bytes32", "paymentAccountHash", 3)]
        public byte[] PaymentAccountHashInit { set { PaymentAccountHash = AppModelConfig.convertToHex(value); } }
        [ApiMember(IsRequired = false, Description = "Bank payment hash as provided by the bank")]
        public string PaymentAccountHash { get; set; }

        [ApiMember(IsRequired = true, Description = "The bond's principal amount")]
        [Parameter("uint", "principal_Cu", 4)]
        public ulong Principal { get; set; }

        [ApiMember(IsRequired = true, Description = "The bond's yield in ppb (parts per billion)")]
        [Parameter("uint", "yield_Ppb", 5)]
        public ulong Yield { get; set; }

        [ApiMember(IsRequired = true, Description = "Amount of funds that will be paid out at a successfull bond maturity")]
        [Parameter("uint", "maturityPayoutAmount_Cu", 6)]
        public ulong MaturityPayoutAmount { get; set; }
        
        [ApiMember(IsRequired = true, Description = "Time the bond has been created")]
        [Parameter("uint", "creationDate", 7)]
        public ulong CreationDate { get; set; }

        [ApiMember(IsRequired = true, Description = "Time by when the bond expires in the current state")]
        [Parameter("uint", "nextStateExpiryDate", 8)]
        public ulong NextStateExpiryDate { get; set; }

        [ApiMember(IsRequired = true, Description = "Time the bond matures or defaults")]
        [Parameter("uint", "maturityDate", 9)]
        public ulong MaturityDate { get; set; }

        [Parameter("uint", "state", 10)]
        public ulong StateInit { set { State = (BondState)value; } }
        [ApiMember(IsRequired = true, Description = "State of the bond")]
        public BondState State { get; set; }

        [Parameter("bytes32", "securityReferenceHash", 11)]
        public byte[] SecurityReferenceHashInit { set { SecurityReferenceHash = AppModelConfig.convertToHex(value); } }
        [ApiMember(IsRequired = false, Description = "Bond hash of the underwritten or underwriting provider")]
        public string SecurityReferenceHash { get; set; }

        [ApiMember(IsRequired = true, Description = "Log entries for this bond")]
        public List<BondLog> Logs {get; set;}
    }

    public class BondLogs {
        [ApiMember(IsRequired = true, Description = "Log entries")]
        public List<BondLog> Logs {get; set;}
    }

    public class BondLog {

        public BondLog(FilterLog fl){
            BlockNumber = Convert.ToUInt64(fl.BlockNumber.HexValue, 16);
            Hash = fl.Topics[1].ToString();
            Owner = AppModelConfig.getAdrFromString32(fl.Topics[2].ToString());
            Timestamp = Convert.ToUInt64(fl.Data.Substring(2 + 0 * 64, 64), 16);
            State = (BondState)Convert.ToInt32(fl.Data.Substring(2 + 1 * 64,64), 16);
            if (AppModelConfig.isEmptyHash(Hash))
                Info = AppModelConfig.FromHexString(fl.Topics[3].ToString());
            else if ((State == BondState.SecuredReferenceBond) || (State == BondState.LockedReferenceBond))
                Info = fl.Topics[3].ToString().EnsureHexPrefix();
            else Info = Convert.ToInt64(fl.Topics[3].ToString(), 16).ToString();
        }

        [ApiMember(IsRequired = true, Description = "The block number this event was triggered")]
        public ulong BlockNumber { get; set; }
        
        [ApiMember(IsRequired = true, Description = "The hash of the Bond")]
        public string Hash { get; set; }

        [ApiMember(IsRequired = true, Description = "The owner of the bond")]
        public string Owner { get; set; }

        [ApiMember(IsRequired = true, Description = "Further information describing the event outcome.")]
        public string Info { get; set; }

        [ApiMember(IsRequired = true, Description = "The timestamp this event was triggered")]
        public ulong Timestamp { get; set; }

        [ApiMember(IsRequired = true, Description = "The new state of the Bond")]
        public BondState State { get; set; }
    }

    public enum BondState {
        /*0*/ Created,
        /*1*/ SecuredBondPrincipal,
        /*2*/ SecuredReferenceBond,
        /*3*/ Signed,
        /*4*/ Issued,
        /*5*/ LockedReferenceBond,
        /*6*/ Defaulted,
        /*7*/ Matured
    }
}