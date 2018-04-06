/**
 * @description Adjustor API endpoint definitions
 * @copyright (c) 2017 Honest Insurance
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using ServiceStack;
using ServiceStack.FluentValidation;


namespace ApiService.ServiceModel
{   
    [Api("Returns a list of adjustors in decending order (latest are at the beginning)")]
    [Route("/adjustor/list", "GET", Summary = "Returns list of adjustors")]
    public class GetAdjustorList : IReturn<AdjustorList>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The maximum number of entries to return (lazy loading)")]
        public ulong MaxEntries { get; set; }
        [ApiMember(IsRequired = false, Description = "For lazy loading purposes, the index of the last Adjustor returned (previously) to continue seaching in the next batch (defaults to last Adjustor Idx)")]
        public ulong FromIdx { get; set; }
    }
    public class GetAdjustorListValidator : AbstractValidator<GetAdjustorList> {
        public GetAdjustorListValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Returns the details for the requested Adjustor")]
    [Route("/adjustor", "GET", Summary = "Returns the details for the requested Adjustor")]
    public class GetAdjustor : IReturn<AdjustorDetail>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the requested Adjustor")]
        public string Hash { get; set; }
        [ApiMember(IsRequired = false, Description = "The index of the requested Adjustor")]
        public ulong Idx { get; set; }
    }
    public class GetAdjustorValidator : AbstractValidator<GetAdjustor> {
        public GetAdjustorValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.Hash).Must((x, hash) => AppModelConfig.validateHash(hash, false))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }

    [Api("Returns a list of adjustor event logs matching the provided search criteria")]
    [Route("/adjustor/logs", "GET", Summary = "Returns a list of the adjustor event logs matching the search parameter")]
    public class GetAdjustorLogs : IReturn<AdjustorLogs>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the adjustor for which to retrieve the event logs for")]
        public string Hash { get; set; }
        [ApiMember(IsRequired = false, Description = "The owner's address for which to retrieve the event logs for")]
        public string Owner { get; set; }
        [ApiMember(IsRequired = false, Description = "Info field for which to retrieve the event logs for")]
        public string Info { get; set; }
        [ApiMember(IsRequired = false, Description = "The starting block from which the logs should be returned from")]
        public ulong FromBlock { get; set; }
        [ApiMember(IsRequired = false, Description = "The last block to which the logs should be returned from")]
        public ulong ToBlock { get; set; }
    }
    public class GetAdjustorLogsValidator : AbstractValidator<GetAdjustorLogs> {
        public GetAdjustorLogsValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.Hash).Must((x, hash) => AppModelConfig.validateHash(hash, false))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.Owner).Must((x, adr) => AppModelConfig.validateAddress(adr, false))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Creates an adjustor within the specified pool ecosystem")]
    [Route("/adjustor", "POST", Summary = "Creates a adjustor and returns the hash of the submitted transaction")]
    public class CreateAdjustor : IReturn<TransactionHash>, IPost {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to sign the transaction with (must be an authorised adjustor's private key)")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = true, Description = "Address of the adjustor's owner")]
        public string Owner { get; set; }
        [ApiMember(IsRequired = true, Description = "The threshold amount this adjustor is authorised to approve settlements")]
        public ulong SettlementApprovalAmount { get; set; }
        [ApiMember(IsRequired = true, Description = "The upper limit this adjustor is authorised to set policie's risk points")]
        public ulong PolicyRiskPointLimit { get; set; }
        [ApiMember(IsRequired = false, Description = "Hash adjustor's service agreement document with the ecosystem")]
        public string ServiceAgreementHash { get; set; }
    }
    public class CreateAdjustorValidator : AbstractValidator<CreateAdjustor> {
        public CreateAdjustorValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.Owner).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.ServiceAgreementHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }

    [Api("Updates an adjustor")]
    [Route("/adjustor", "PUT", Summary = "Updates a adjustor and returns the hash of the submitted transaction")]
    public class UpdateAdjustor : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to sign the transaction with (must be an authorised adjustor's private key)")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the adjustor updating this adjustor (must match the signing key used)")]
        public string AdjustorHash { get; set; }
        [ApiMember(IsRequired = true, Description = "Address of the adjustor's owner")]
        public string Owner { get; set; }
        [ApiMember(IsRequired = true, Description = "The threshold amount this adjustor is authorised to approve settlements")]
        public ulong SettlementApprovalAmount { get; set; }
        [ApiMember(IsRequired = true, Description = "The upper limit this adjustor is authorised to set policie's risk points")]
        public ulong PolicyRiskPointLimit { get; set; }
        [ApiMember(IsRequired = false, Description = "Hash adjustor's service agreement document with the ecosystem")]
        public string ServiceAgreementHash { get; set; }
    }
    public class UpdateAdjustorValidator : AbstractValidator<UpdateAdjustor> {
        public UpdateAdjustorValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.AdjustorHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.Owner).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.ServiceAgreementHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }

    [Api("Allows the adjustor holder to permanently retire this adjustor")]
    [Route("/adjustor/retire", "PUT", Summary = "Retires this adjustor (can only be performed by the adjustor holder)")]
    public class RetireAdjustor : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to sign the transaction with (must be an authorised adjustor's private key)")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the adjustor to be retired")]
        public string AdjustorHash { get; set; }
    }
    public class RetireAdjustorValidator : AbstractValidator<RetireAdjustor> {
        public RetireAdjustorValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.AdjustorHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }
}