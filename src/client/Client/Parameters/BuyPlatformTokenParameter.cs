using Client.Infrastructure.Extensions;
using FluentValidation;
using Neo;

namespace Client.Parameters
{
    public class BuyPlatformTokenParameter
    {
        public UInt160 CurrencyHash { get; set; }
        public string Currency { get; set; }
        public int CurrencyDecimals { get; set; }
        public string WalletAddress { get; set; }
        public double Amount { get; set; }
    }

    public class BuyPlatformTokenParameterValidator : AbstractValidator<BuyPlatformTokenParameter>
    {
        public BuyPlatformTokenParameterValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(v => v.Currency)
                .NotNull()
                .NotEmpty();

            RuleFor(v => v.Amount)
                .NotEmpty().WithMessage("Amount field must not be empty and should be in correct format (dot for decimal places).")
                .GreaterThan(0.0).WithMessage("Amount must be greater than 0");

        }
    }
}
