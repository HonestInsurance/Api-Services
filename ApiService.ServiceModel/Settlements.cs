/**
 * @description Settlement API endpoint definitions
 * @copyright (c) 2017 Honest Insurance
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using ServiceStack;
using ServiceStack.FluentValidation;


namespace ApiService.ServiceModel
{   
    [Api("Returns a list of settlements in decending order (latest are at the beginning)")]
    [Route("/settlement/list", "GET", Summary = "Returns list of settlements")]
    public class GetSettlementList : IReturn<SettlementList>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The maximum number of entries to return (lazy loading)")]
        public ulong MaxEntries { get; set; }
        [ApiMember(IsRequired = false, Description = "For lazy loading purposes, the index of the last Settlement returned (previously) to continue seaching in the next batch (defaults to last Settlement Idx)")]
        public ulong FromIdx { get; set; }
    }
    public class GetSettlementListValidator : AbstractValidator<GetSettlementList> {
        public GetSettlementListValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Returns the details for the requested Settlement")]
    [Route("/settlement", "GET", Summary = "Returns the details for the requested Settlement")]
    public class GetSettlement : IReturn<SettlementDetail>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the requested Settlement")]
        public string Hash { get; set; }
        [ApiMember(IsRequired = false, Description = "The index of the requested Settlement")]
        public ulong Idx { get; set; }
    }
    public class GetSettlementValidator : AbstractValidator<GetSettlement> {
        public GetSettlementValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.Hash).Must((x, hash) => AppModelConfig.validateHash(hash, false))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }

    [Api("Returns a list of settlement event logs matching the provided search criteria")]
    [Route("/settlement/logs", "GET", Summary = "Returns a list of the settlement event logs matching the search parameter")]
    public class GetSettlementLogs : IReturn<PolicyLogs>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the policy for which to retrieve the event logs for")]
        public string SettlementHash { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the adjustor this event log is created by")]
        public string AdjustorHash { get; set; }
        [ApiMember(IsRequired = false, Description = "Info field for which to retrieve the event logs for")]
        public string Info { get; set; }
        [ApiMember(IsRequired = false, Description = "The starting block from which the logs should be returned from")]
        public ulong FromBlock { get; set; }
        [ApiMember(IsRequired = false, Description = "The last block to which the logs should be returned from")]
        public ulong ToBlock { get; set; }
    }
    public class GetSettlementLogsValidator : AbstractValidator<GetSettlementLogs> {
        public GetSettlementLogsValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SettlementHash).Must((x, hash) => AppModelConfig.validateHash(hash, false))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.AdjustorHash).Must((x, hash) => AppModelConfig.validateHash(hash, false))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }

    [Api("Creates a settlement within the specified pool ecosystem")]
    [Route("/settlement", "POST", Summary = "Creates a settlement and returns the hash of the submitted transaction")]
    public class CreateSettlement : IReturn<TransactionHash>, IPost {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key of an adjustor sign the transaction with")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = true, Description = "Hash of the adjustor creating this settlement")]
        public string AdjustorHash { get; set; }
        [ApiMember(IsRequired = false, Description = "Hash of the policy this settlement refers to")]
        public string PolicyHash { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the initial settlement document")]
        public string DocumentHash { get; set; }
    }
    public class CreateSettlementValidator : AbstractValidator<CreateSettlement> {
        public CreateSettlementValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.AdjustorHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.PolicyHash).Must((x, hash) => AppModelConfig.validateHash(hash, false))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.DocumentHash).Must((x, hash) => AppModelConfig.validateHash(hash, false))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }

    [Api("Add additional information to a settlement")]
    [Route("/settlement/addinfo", "PUT", Summary = "Updates a settlement with additional information")]
    public class AddSettlementInfo : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key of an adjustor sign the transaction with")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = false, Description = "Hash of the settlement to be updated")]
        public string SettlementHash { get; set; }
        [ApiMember(IsRequired = true, Description = "Hash of the adjustor creating this settlement")]
        public string AdjustorHash { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the initial settlement document")]
        public string DocumentHash { get; set; }
    }
    public class AddSettlementInfoValidator : AbstractValidator<AddSettlementInfo> {
        public AddSettlementInfoValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.SettlementHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.AdjustorHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.DocumentHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }

    [Api("Closes a settlement")]
    [Route("/settlement/close", "PUT", Summary = "Updates a settlement with additional information and closes the settlement")]
    public class CloseSettlement : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key of an adjustor sign the transaction with")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = false, Description = "Hash of the settlement to be updated")]
        public string SettlementHash { get; set; }
        [ApiMember(IsRequired = true, Description = "Hash of the adjustor creating this settlement")]
        public string AdjustorHash { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the initial settlement document")]
        public string DocumentHash { get; set; }
        [ApiMember(IsRequired = true, Description = "The final settlement amount")]
        public ulong SettlementAmount { get; set; }
    }
    public class CloseSettlementValidator : AbstractValidator<CloseSettlement> {
        public CloseSettlementValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.SettlementHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.AdjustorHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.DocumentHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }

    [Api("Updates the expected settlement amount for an open and active settlement")]
    [Route("/settlement/amount", "PUT", Summary = "Updates the expected settlement amount for an active settlement")]
    public class SetExpectedSettlementAmount : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key of an adjustor sign the transaction with")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = false, Description = "Hash of the settlement to be updated")]
        public string SettlementHash { get; set; }
        [ApiMember(IsRequired = true, Description = "Hash of the adjustor creating this settlement")]
        public string AdjustorHash { get; set; }
        [ApiMember(IsRequired = true, Description = "The expected settlement amount for this settlement")]
        public ulong ExpectedSettlementAmount { get; set; }
    }
    public class SetExpectedSettlementAmountValidator : AbstractValidator<SetExpectedSettlementAmount> {
        public SetExpectedSettlementAmountValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.SettlementHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.AdjustorHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }
}