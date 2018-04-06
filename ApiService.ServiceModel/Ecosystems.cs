/**
 * @description Ecosystem API endpoint definitions
 * @copyright (c) 2017 Honest Insurance
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using ServiceStack;
using ServiceStack.FluentValidation;


namespace ApiService.ServiceModel
{  
    // ********************************************************************************************
    // *** ECOSYSTEM
    // ********************************************************************************************

    [Api("Returns the ecosystem's variables from all the contracts")]
    [Route("/ecosystem/status", "GET", Summary = "Status and value of the ecosystem's current values such as bank account balances, current day, etc. ")]
    public class GetEcosystemStatus : IReturn<EcosystemStatus>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
    }
    public class GetEcosystemStatusValidator : AbstractValidator<GetEcosystemStatus> {
        public GetEcosystemStatusValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Returns a list of ecosystem event logs matching the provided search criteria")]
    [Route("/ecosystem/logs", "GET", Summary = "Returns a list of ecosystem relevant event logs matching the search parameter")]
    public class GetEcosystemLogs : IReturn<EcosystemLogs>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The subject describing the event")]
        public string Subject { get; set; }
        [ApiMember(IsRequired = false, Description = "The day that was specified for the event log")]
        public ulong Day { get; set; }
        [ApiMember(IsRequired = false, Description = "Value field for which to retrieve the event logs for")]
        public ulong Value { get; set; }
        [ApiMember(IsRequired = false, Description = "The starting block from which the logs should be returned from")]
        public ulong FromBlock { get; set; }
        [ApiMember(IsRequired = false, Description = "The last block to which the logs should be returned from")]
        public ulong ToBlock { get; set; }
    }
    public class GetEcosystemLogsValidator : AbstractValidator<GetEcosystemLogs> {
        public GetEcosystemLogsValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Returns the constants that are being used withing insurance ecosystem")]
    [Route("/ecosystem/configuration", "GET", Summary = "The constants (or parameters) used within this ecosystem")]
    public class GetEcosystemConfiguration : IReturn<EcosystemConfiguration>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
    }
    public class GetEcosystemConfigurationValidator : AbstractValidator<GetEcosystemConfiguration> {
        public GetEcosystemConfigurationValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Returns the addresses of all the smart contracts that are deployed and form an insurance pool ecosystem ")]
    [Route("/ecosystem/contractaddresses", "GET", Summary = "Addresses of all the contracts that form this insurance ecosystem")]
    public class GetEcosystemContractAddresses : IReturn<EcosystemContractAddresses>, IGet {
        [ApiMember(IsRequired = true, Description = "The contract address to retrieve the configured ecosystem addresses from")]
        public string ContractAdr { get; set; }
    }
    public class GetEcosystemContractAddressesValidator : AbstractValidator<GetEcosystemContractAddresses> {
        public GetEcosystemContractAddressesValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    // ********************************************************************************************
    // *** TRUST
    // ********************************************************************************************

    [Api("Returns a list of trust event logs matching the provided search criteria")]
    [Route("/trust/logs", "GET", Summary = "Returns a list of the trust event logs matching the search parameter")]
    public class GetTrustLogs : IReturn<TrustLogs>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The subject describing the event")]
        public string Subject { get; set; }
        [ApiMember(IsRequired = false, Description = "The address info on this log entry")]
        public string Address { get; set; }
        [ApiMember(IsRequired = false, Description = "Further info describing the event outcome")]
        public string Info { get; set; }
        [ApiMember(IsRequired = false, Description = "The starting block from which the logs should be returned from")]
        public ulong FromBlock { get; set; }
        [ApiMember(IsRequired = false, Description = "The last block to which the logs should be returned from")]
        public ulong ToBlock { get; set; }
    }
    public class GetTrustLogsValidator : AbstractValidator<GetTrustLogs> {
        public GetTrustLogsValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.Address).Must((x, adr) => AppModelConfig.validateAddress(adr, false))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    // ********************************************************************************************
    // *** ECOSYSTEM TRANSACTIONS - Set Working Capital expenses, Adjust daylight saving, etc.
    // ********************************************************************************************

    [Api("Configures the Exensens of the insurance pool ecosystem for the current day")]
    [Route("/ecosystem/setWcExpenses", "PUT", Summary = "Sets the current day expenses to be used for next overnight processing")]
    public class SetWcExpenses : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The trust's private key to sign the transaction with")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = true, Description = "The amount in FC to set the Working capital expenses to")]
        public ulong Amount { get; set; }
    } 
    public class SetWcExpensesValidator : AbstractValidator<SetWcExpenses> {
        public SetWcExpensesValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
        }
    }

    [Api("Sets the flag to account for a daylight saving change during the next overnight processing")]
    [Route("/ecosystem/adjustDaylightSaving", "PUT", Summary = "If set a change of summer/winter time during next overnight processing occurs")]
    public class AdjustDaylightSaving : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The trusts's private key to sign the transaction with")]
        public string SigningPrivateKey { get; set; }
    }
    
    public class AdjustDaylightSavingValidator : AbstractValidator<AdjustDaylightSaving> {
        public AdjustDaylightSavingValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
        }
    }

    // ********************************************************************************************
    // *** HOSTING ENVIRONMENT AND CONFIGURATION SETUP
    // ********************************************************************************************

    [Api("Returns the configuration details for this hosting environment used")]
    [Route("/config", "GET", Summary = "Returns the hosting environment configuration details for this API service")]
    public class HostingEnvironmentSetup : IReturn<HostingEnvironment>, IGet {
    }

    // ********************************************************************************************
    // *** EXTERNAL ACCESS KEYS - AuthKeys management
    // ********************************************************************************************

    [Api("Returns the configured authorisation keys either for the bank or trust contract")]
    [Route("/authkeys", "GET", Summary = "Authorisation keys configured in the Bank or Trust contract")]
    public class GetContractAuthKeys : IReturn<ContractAuthKeys>, IGet {
        [ApiMember(IsRequired = true, Description = "The contract address of the Bank or Trust contract")]
        public string ContractAdr { get; set; }
    }
    public class GetContractAuthKeysValidator : AbstractValidator<GetContractAuthKeys> {
        public GetContractAuthKeysValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Performs a preauthorisation with the key provided")]
    [Route("/authkeys/preauth", "PUT", Summary = "Performs a preauthorisation with the provided private key")]
    public class PreAuth : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the contract to perform a pre-authorisation")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to perform the preauthorisation with")]
        public string SigningPrivateKey { get; set; }
    }
    public class PreAuthValidator : AbstractValidator<PreAuth> {
        public PreAuthValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
        }
    }

    [Api("Adds a new key to the authorised keys list")]
    [Route("/authkeys/addauthkey", "PUT", Summary = "Adds the address of an externally held account (i.e. not a contract's address)")]
    public class AddAuthKey : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the contract to add the key")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to sign the transaction with")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = true, Description = "The public key to add to the authorised key list (contract address is not permitted)")]
        public string KeyToAddAdr { get; set; }
    }
    public class AddAuthKeyValidator : AbstractValidator<AddAuthKey> {
        public AddAuthKeyValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
            RuleFor(x => x.KeyToAddAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Performs a rotation of the keys for the specified contract")]
    [Route("/authkeys/rotateauthkey", "PUT", Summary = "Performs a rotation of the keys for the specified contract")]
    public class RotateAuthKey : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of the contract to perform a key rotation")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to perform the key rotation with")]
        public string SigningPrivateKey { get; set; }
    }
    public class RotateAuthKeyValidator : AbstractValidator<RotateAuthKey> {
        public RotateAuthKeyValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
        }
    }
}