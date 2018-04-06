/**
 * @description Timer API endpoint definitions
 * @copyright (c) 2017 Honest Insurance
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using ServiceStack;
using ServiceStack.FluentValidation;


namespace ApiService.ServiceModel
{
    [Api("Returns a list of timer notification events scheduled")]
    [Route("/timer/notifications", "GET", Summary = "Returns a list of the scheduled notification events for the time period specified")]
    public class GetTimerNotifications : IReturn<TimerNotifications>, IGet {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = false, Description = "The starting time from which the notifications should be returned from")]
        public ulong FromTime { get; set; }
        [ApiMember(IsRequired = false, Description = "The end time to which the notifications should be returned from")]
        public ulong ToTime { get; set; }
    }
    public class GetTimerNotificationsValidator : AbstractValidator<GetTimerNotifications> {
        public GetTimerNotificationsValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
        }
    }

    [Api("Calls the timer contract to perform a ping calling all pending transactions to be executed")]
    [Route("/timer/ping", "PUT", Summary = "Causes all outstanding transactions to be executed as scheduled in the timer contract")]
    public class PingTimer : IReturn<TransactionHash>, IPut {
        [ApiMember(IsRequired = true, Description = "The address of any contract belonging to this ecosystem")]
        public string ContractAdr { get; set; }
        [ApiMember(IsRequired = true, Description = "The private key to sign and pay for the ping transaction with")]
        public string SigningPrivateKey { get; set; }
        [ApiMember(IsRequired = true, Description = "Duration in seconds to schedule auto ping functionality repeatable")]
        public ulong AutoSchedulePingDuration { get; set; }
    }
    
    public class PingTimerValidator : AbstractValidator<PingTimer> {
        public PingTimerValidator() {
            RuleFor(x => x.ContractAdr).Must((x, adr) => AppModelConfig.validateAddress(adr, true))
                .WithErrorCode(AppModelConfig.AddressError)
                .WithMessage(AppModelConfig.AddressErrorMessage);
            RuleFor(x => x.SigningPrivateKey).Must((x, privateKey) => AppModelConfig.validatePrivateKey(privateKey, true))
                .WithErrorCode(AppModelConfig.PrivateKeyError)
                .WithMessage(AppModelConfig.PrivateKeyErrorMessage);
        }
    }
}