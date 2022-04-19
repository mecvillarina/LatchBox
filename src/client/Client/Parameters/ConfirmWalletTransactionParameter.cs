using FluentValidation;

namespace Client.Parameters
{
    public class ConfirmWalletTransactionParameter
    {
        public string WalletAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ConfirmWalletTransactionParameterValidator : AbstractValidator<ConfirmWalletTransactionParameter>
    {
        public ConfirmWalletTransactionParameterValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(v => v.Password)
              .NotEmpty().WithMessage("Please input your wallet address");

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage("Please input password");
        }
    }
}
