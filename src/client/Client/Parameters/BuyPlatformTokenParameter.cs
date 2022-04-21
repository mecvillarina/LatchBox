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
                //.NotEmpty().WithMessage("'Amount' must not be empty and decimal places should be in dot format.")
                .GreaterThan(0.0).WithMessage("'Amount' must be greater than 0");

        }
    }
}
