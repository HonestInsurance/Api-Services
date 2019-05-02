/**
 * @description Bank Data Transfer Objects (DTOs)
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
    public class BankLogs {
        [ApiMember(IsRequired = true, Description = "Bank transaction log entries")]
        public List<BankLog> Logs {get; set;}
    }

    public class BankLog : IParseLog {

        public void parseLog(FilterLog fl) {
            BlockNumber = Convert.ToUInt64(fl.BlockNumber.HexValue, 16);
            InternalReferenceHash = fl.Topics[1].ToString();
            AccountType = (AccountType)Convert.ToUInt64(fl.Topics[2].ToString(), 16);
            Success = fl.Topics[3].ToString().EndsWith("1");
            PaymentAccountHash = fl.Data.Substring(2 + 0 * 64, 64).EnsureHexPrefix();
            PaymentSubject = fl.Data.Substring(2 + 1 * 64, 64).EnsureHexPrefix().StartsWith("0x000000") ?
                Convert.ToUInt64(fl.Data.Substring(2 + 1 * 64, 64), 16).ToString() :
                fl.Data.Substring(2 + 1 * 64, 64).EnsureHexPrefix();
            Info = AppModelConfig.isEmptyHash(fl.Data.Substring(2 + 2 * 64, 64).EnsureHexPrefix()) ? "0x0" : AppModelConfig.FromHexString(fl.Data.Substring(2 + 2 * 64, 64));
            Timestamp = Convert.ToUInt64(fl.Data.Substring(2 + 3 * 64, 64), 16);
            TransactionType = (TransactionType)Convert.ToUInt64(fl.Data.Substring(2 + 4 * 64, 64), 16);
            Amount = Convert.ToUInt64(fl.Data.Substring(2 + 5 * 64, 64), 16);
        }

        [ApiMember(IsRequired = true, Description = "The block number this event was triggered")]
        public ulong BlockNumber { get; set; }
        
        [ApiMember(IsRequired = true, Description = "Hash describing the internal object this bank transaction belongs to")]
        public string InternalReferenceHash { get; set; }

        [ApiMember(IsRequired = true, Description = "The account type this bank transaction belongs to")]
        public AccountType AccountType { get; set; }

        [ApiMember(IsRequired = true, Description = "Describes if the event outcome was successfull or not")]
        public bool Success { get; set; }

        [ApiMember(IsRequired = true, Description = "The hash that was created by the bank being a unique reference to bank account owner and number")]
        public string PaymentAccountHash { get; set; }

        [ApiMember(IsRequired = true, Description = "The subject (payment reference) that was specified in the bank transaction payment")]
        public string PaymentSubject { get; set; }

        [ApiMember(IsRequired = true, Description = "Further information describing the transaction outcome if applicable")]
        public string Info { get; set; }

        [ApiMember(IsRequired = true, Description = "The timestamp this event was triggered")]
        public ulong Timestamp { get; set; }

        [ApiMember(IsRequired = true, Description = "The transaction type (debit/credit) of this bank transaction")]
        public TransactionType TransactionType { get; set; }

        [ApiMember(IsRequired = true, Description = "The amount that was transacted with this bank transaction")]
        public ulong Amount { get; set; }
    }

    [FunctionOutput]
    public class PaymentAdviceList {
        [ApiMember(IsRequired = true, Description = "List of the items")]
        public List<PaymentAdviceDetail> Items { get; set;}
    }

    [FunctionOutput]
    public class PaymentAdviceDetail {
        [ApiMember(IsRequired = true, Description = "The index of this payment advice")]
        public ulong Idx { get; set; }

        [Parameter("uint", "adviceType", 1)]
        public ulong AdviceTypeInit { set { AdviceType = (PaymentAdviceType)value; } }
        [ApiMember(IsRequired = true, Description = "Type of the requested payment advice - Enum: PremiumRefund, Premium, BondMaturity, Overflow, CashSettlement, ServiceProvider, SafetyNet, PoolOperator")]
        public PaymentAdviceType AdviceType { get; set; }

        [Parameter("bytes32", "paymentAccountHashRecipient", 2)]
        public byte[] PaymentAccountHashRecipientInit { set { PaymentAccountHashRecipient = AppModelConfig.convertToHex(value); } }
        [ApiMember(IsRequired = true, Description = "Hash of the payment account details of the receiver of the funds")]
        public string PaymentAccountHashRecipient { get; set; }

        [Parameter("bytes32", "paymentSubject", 3)]
        public byte[] PaymentSubjectInit { set { PaymentSubject = AppModelConfig.convertToHex(value); } }
        [ApiMember(IsRequired = true, Description = "The subject (payment reference) to be specified in the bank transaction payment")]
        public string PaymentSubject { get; set; }

        [ApiMember(IsRequired = true, Description = "Transaction amount")]
        [Parameter("uint", "amount", 4)]
        public ulong Amount { get; set; }

        [Parameter("bytes32", "internalReferenceHash", 5)]
        public byte[] InternalReferenceHashInit { set { InternalReferenceHash = AppModelConfig.convertToHex(value); } }
        [ApiMember(IsRequired = true, Description = "Hash describing the internal object this bank transaction belongs to")]
        public string InternalReferenceHash { get; set; }
    }


    public enum SuccessFilter {
        All,
        Positive,
        Negative
    }

    // Enum of the diffent Bank accounts held by the pool
    public enum PaymentAdviceType {
        /*0*/ PremiumRefund,
        /*1*/ Premium,
        /*2*/ BondMaturity,
        /*3*/ Overflow,
        /*4*/ PoolOperator,
        /*5*/ ServiceProvider,
        /*6*/ Trust
    }

    // Enum of the diffent Bank accounts held by the pool
    public enum AccountType {
        /*0*/ PremiumAccount,
        /*1*/ BondAccount,
        /*2*/ FundingAccount
    }

    // Enum of the two types of bank transactions (Credit; Debit)
    public enum TransactionType {
        /*0*/ Credit,
        /*1*/ Debit
    }
}