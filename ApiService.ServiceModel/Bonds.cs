/**
 * @description Bond API endpoint definitions
 * @copyright (c) 2017 Honest Insurance
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using ServiceStack;
using ServiceStack.FluentValidation;


namespace ApiService.ServiceModel
{
    [Api("Returns a list of bonds in decending order (latest are at the beginning)")]
    [Route("/bond/list", "GET", Summary = "Returns list of bonds")]
    public class GetBondList : IReturn<BondList>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The bond owner's address for which to retrieve all the bonds for (defaults to all)")]
        public string Owner { get; set; }
        [ApiMember(IsRequired = false, Description = "The maximum number of entries to return (lazy loading)")]
        public ulong MaxEntries { get; set; }
        [ApiMember(IsRequired = false, Description = "For lazy loading purposes, the index of the last Bond returned (previously) to continue seaching in the next batch (defaults to last Bond Idx)")]
        public ulong FromIdx { get; set; }
    }
    public class GetBondListValidator : AbstractValidator<GetBondList> {
        public GetBondListValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.Owner).Must((x, adr) => AppModelConfig.validateAddress(adr, false))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Returns the details for the requested Bond")]
    [Route("/bond", "GET", Summary = "Returns the details for the requested Bond")]
    public class GetBond : IReturn<BondDetail>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the requested Bond")]
        public string Hash { get; set; }
        [ApiMember(IsRequired = false, Description = "The index of the requested Bond")]
        public ulong Idx { get; set; }
    }
    public class GetBondValidator : AbstractValidator<GetBond> {
        public GetBondValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.Hash).Must((x, hash) => AppModelConfig.validateHash(hash, false))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }

    [Api("Returns a list of bond event logs matching the provided search criteria")]
    [Route("/bond/logs", "GET", Summary = "Returns a list of the bond event logs matching the search parameter")]
    public class GetBondLogs : IReturn<BondLogs>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the bond for which to retrieve the event logs for")]
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
    public class GetBondLogsValidator : AbstractValidator<GetBondLogs> {
        public GetBondLogsValidator() {
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

    [Api("Creates a bond within the specified pool ecosystem")]
    [Route("/bond", "POST", Summary = "Creates a bond and returns the hash of the submitted transaction")]
    public class CreateBond : IReturn<TransactionHash>, IPost {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to sign the transaction with")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = true, Description = "The requested bond principal")]
        public ulong Principal { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the bond that is used as a security for the new bond (this is an optional parameter)")]
        public string SecurityReferenceHash { get; set; }
    }
    public class CreateBondValidator : AbstractValidator<CreateBond> {
        public CreateBondValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.Principal).NotEmpty();
            RuleFor(x => x.SecurityReferenceHash).Must((x, hash) => AppModelConfig.validateHash(hash, false))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }
}