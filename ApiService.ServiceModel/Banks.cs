/**
 * @description Bank API endpoint definitions
 * @copyright (c) 2017 Honest Insurance
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using ServiceStack;
using ServiceStack.FluentValidation;


namespace ApiService.ServiceModel
{
    [Api("Returns a list of bank transaction event logs matching the provided search criteria")]
    [Route("/bank/logs", "GET", Summary = "Returns a list of bank transaction event logs matching the provided search criteria")]
    public class GetBankLogs : IReturn<BankLogs>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "Internal transaction hash referenced for which to retrieve the event logs for")]
        public string InternalReferenceHash { get; set; }
        [ApiMember(IsRequired = false, Description = "The account type this bank transaction belongs to (0-PremiumAccount, 1-BondAccount or 2-FundingAccount)")]
        public AccountType AccountType { get; set; }
        [ApiMember(IsRequired = false, Description = "Event outcome success flag for which to retrieve the event logs for (All, Positive or Negative)")]
        public SuccessFilter Success { get; set; }
        [ApiMember(IsRequired = false, Description = "The starting block from which the logs should be returned from")]
        public ulong FromBlock { get; set; }
        [ApiMember(IsRequired = false, Description = "The last block to which the logs should be returned from")]
        public ulong ToBlock { get; set; }
    }
    public class GetBankLogsValidator : AbstractValidator<GetBankLogs> {
        public GetBankLogsValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.InternalReferenceHash).Must((x, hash) => AppModelConfig.validateHash(hash, false))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }

    [Api("Returns a list of all outstanding bank payment advice waiting to be processed")]
    [Route("/bank/paymentadvice", "GET", Summary = "List of all outstanding payment advice entries")]
    public class GetBankPaymentAdviceList : IReturn<PaymentAdviceList>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
    }
    public class GetPaymentAdviceValidator : AbstractValidator<GetBankPaymentAdviceList> {
        public GetPaymentAdviceValidator() {
            RuleFor(b => b.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Confirms the specified payment advice as processed")]
    [Route("/bank/processpaymentadvice", "POST", Summary = "Marks the specified payment advice as processed")]
    public class ProcessBankPaymentAdvice : IReturn<TransactionHash>, IPost {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to sign the transaction with")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = true, Description = "The index of the bank payment advice to return")]
        public ulong AdviceIdx { get; set; }
        [ApiMember(IsRequired = true, Description = "The transaction index that was used by the bank internally (used to prevent double processing)")]
        public ulong BankTransactionIdx { get; set; }
    }
    public class ProcessPaymentAdviceValidator : AbstractValidator<ProcessBankPaymentAdvice> {
        public ProcessPaymentAdviceValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
        }
    }

    [Api("Confirms a payment (credit) has been received at one of the bank accounts (Premium, Bond or Funding)")]
    [Route("/bank/processaccountcredit", "POST", Summary = "Confirms a payment (credit) has been received at one of the bank accounts (Premium, Bond or Funding)")]
    public class ProcessBankAccountCredit : IReturn<TransactionHash>, IPost {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to sign the transaction with")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = true, Description = "The transaction index that was used by the bank internally (used to prevent double processing)")]
        public ulong BankTransactionIdx { get; set; }
        [ApiMember(IsRequired = true, Description = "The account type this bank transaction belongs to (PremiumAccount, BondAccount or FundingAccount)")]
        public AccountType AccountType { get; set; }
        [ApiMember(IsRequired = true, Description = "Hash of the payment account details of the sender of the funds")]
        public string PaymentAccountHashSender { get; set; }
        [ApiMember(IsRequired = true, Description = "The subject (payment reference) that has been provided with the payment")]
        public string PaymentSubject { get; set; }
        [ApiMember(IsRequired = true, Description = "The amount that has been credited")]
        public ulong CreditAmount { get; set; }
    }
    public class ProcessAccountCreditValidator : AbstractValidator<ProcessBankAccountCredit> {
        public ProcessAccountCreditValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.PaymentAccountHashSender).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.CreditAmount).GreaterThan((ulong)0);
        }
    }
}