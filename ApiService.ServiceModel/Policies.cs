/**
 * @description Policy API endpoint definitions
 * @copyright (c) 2017 HIC Limited (NZBN: 9429043400973)
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using ServiceStack;
using ServiceStack.FluentValidation;


namespace ApiService.ServiceModel
{   
    [Api("Returns a list of policies in decending order (latest are at the beginning)")]
    [Route("/policy/list", "GET", Summary = "Returns list of policies")]
    public class GetPolicyList : IReturn<PolicyList>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The policy owner's address for which to retrieve all the policies for (defaults to all)")]
        public string Owner { get; set; }
        [ApiMember(IsRequired = false, Description = "The maximum number of entries to return (lazy loading)")]
        public ulong MaxEntries { get; set; }
        [ApiMember(IsRequired = false, Description = "For lazy loading purposes, the index of the last Bond returned (previously) to continue seaching in the next batch (defaults to last Bond Idx)")]
        public ulong FromIdx { get; set; }
    }
    public class GetPolicyListValidator : AbstractValidator<GetPolicyList> {
        public GetPolicyListValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.Owner).Must((x, adr) => AppModelConfig.validateAddress(adr, false))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Returns the details for the requested Policy")]
    [Route("/policy", "GET", Summary = "Returns the details for the requested Policy")]
    public class GetPolicy : IReturn<PolicyDetail>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the requested policy")]
        public string Hash { get; set; }
        [ApiMember(IsRequired = false, Description = "The index of the requested policy")]
        public ulong Idx { get; set; }
    }
    public class GetPolicyValidator : AbstractValidator<GetPolicy> {
        public GetPolicyValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.Hash).Must((x, hash) => AppModelConfig.validateHash(hash, false))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }

    [Api("Returns a list of policy event logs matching the provided search criteria")]
    [Route("/policy/logs", "GET", Summary = "Returns a list of the policy event logs matching the search parameter")]
    public class GetPolicyLogs : IReturn<PolicyLogs>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the policy for which to retrieve the event logs for")]
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
    public class GetPolicyLogsValidator : AbstractValidator<GetPolicyLogs> {
        public GetPolicyLogsValidator() {
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

    [Api("Creates a policy within the specified pool ecosystem")]
    [Route("/policy", "POST", Summary = "Creates a policy and returns the hash of the submitted transaction")]
    public class CreatePolicy : IReturn<TransactionHash>, IPost {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to sign the transaction with (must be an authorised adjustor's private key)")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the adjustor creating this policy (must match the signing key used)")]
        public string AdjustorHash { get; set; }
        [ApiMember(IsRequired = false, Description = "The address of the owner of this policy")]
        public string Owner { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the policy document")]
        public string DocumentHash { get; set; }
        [ApiMember(IsRequired = true, Description = "The risk points associated with this policy")]
        public ulong RiskPoints { get; set; }
    }
    public class CreatePolicyValidator : AbstractValidator<CreatePolicy> {
        public CreatePolicyValidator() {
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
            RuleFor(x => x.DocumentHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.RiskPoints).NotEmpty();
        }
    }

    [Api("Updates a policy")]
    [Route("/policy", "PUT", Summary = "Updates a policy and returns the hash of the submitted transaction")]
    public class UpdatePolicy : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to sign the transaction with (must be an authorised adjustor's private key)")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the adjustor updating this policy (must match the signing key used)")]
        public string AdjustorHash { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the policy to be updated")]
        public string PolicyHash { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the policy document")]
        public string DocumentHash { get; set; }
        [ApiMember(IsRequired = true, Description = "The risk points associated with this policy")]
        public ulong RiskPoints { get; set; }
    }
    public class UpdatePolicyValidator : AbstractValidator<UpdatePolicy> {
        public UpdatePolicyValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.AdjustorHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.PolicyHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.DocumentHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
            RuleFor(x => x.RiskPoints).NotEmpty();
        }
    }

    [Api("Allows the policy holder to temporarily suspend a policy")]
    [Route("/policy/suspend", "PUT", Summary = "Suspends a policy (can only be performed by the policy holder)")]
    public class SuspendPolicy : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to sign the transaction with (must be an authorised adjustor's private key)")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the policy to be suspended")]
        public string PolicyHash { get; set; }
    }
    public class SuspendPolicyValidator : AbstractValidator<SuspendPolicy> {
        public SuspendPolicyValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.PolicyHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Allows the policy holder to unsuspend a policy if it is in a suspended state")]
    [Route("/policy/unsuspend", "PUT", Summary = "Reverts the policy suspension (can only be performed by the policy holder)")]
    public class UnsuspendPolicy : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to sign the transaction with (must be an authorised adjustor's private key)")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the policy to be unsuspended")]
        public string PolicyHash { get; set; }
    }
    public class UnsuspendPolicyValidator : AbstractValidator<UnsuspendPolicy> {
        public UnsuspendPolicyValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.PolicyHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Allows the policy holder to permanently retire this policy")]
    [Route("/policy/retire", "PUT", Summary = "Retires this policy (can only be performed by the policy holder)")]
    public class RetirePolicy : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to sign the transaction with (must be an authorised adjustor's private key)")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = false, Description = "The hash of the policy to be retired")]
        public string PolicyHash { get; set; }
    }
    public class RetirePolicyValidator : AbstractValidator<RetirePolicy> {
        public RetirePolicyValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.PolicyHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }
}