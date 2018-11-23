/**
 * @description Transaction API endpoint definitions
 * @copyright (c) 2017 HIC Limited (NZBN: 9429043400973)
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using ServiceStack;
using ServiceStack.FluentValidation;


namespace ApiService.ServiceModel
{   
    [Api("Returns the transaction receipt")]
    [Route("/transaction", "GET", Summary = "Returns the transaction receipt for the specified tansaction hash")]
    public class GetReceipt : IReturn<TransactionResult>, IGet {
        [ApiMember(IsRequired = true, Description = "The hash of the transaction")]
        public string TransactionHash { get; set; }
        [ApiMember(IsRequired = false, Description = "The maximum duration in seconds to wait for a pending transaction to be mined before returning")]
        public ulong MaxWaitDurationForTransactionReceipt { get; set; }
    }
    public class GetReceiptValidator : AbstractValidator<GetReceipt> {
        public GetReceiptValidator() {
            RuleFor(x => x.TransactionHash).Must((x, hash) => AppModelConfig.validateHash(hash, true))
                .WithErrorCode(AppModelConfig.HashError)
                .WithMessage(AppModelConfig.HashErrorMessage);
        }
    }
}