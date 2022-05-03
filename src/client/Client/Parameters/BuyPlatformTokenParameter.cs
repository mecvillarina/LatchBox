using Client.Infrastructure.Extensions;
using FluentValidation;
using Neo;

namespace Client.Parameters
{
    public class BuyPlatformTokenParameter
    {
        public string WalletAddress { get; set; }
        public double Amount { get; set; }
    }

    public class BuyPlatformTokenParameterValidator : AbstractValidator<BuyPlatformTokenParameter>
    {
        public BuyPlatformTokenParameterValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(v => v.Amount)
                .GreaterThan(0.0).WithMessage("'Amount' must be greater than 0");

        }
    }
}
