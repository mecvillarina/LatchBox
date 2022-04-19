using FluentValidation;

namespace Client.Parameters
{
    public class ConnectWalletParameter
    {
        public string Password { get; set; } = string.Empty;
    }

    public class ConnectWalletParameterValidator : AbstractValidator<ConnectWalletParameter>
    {
        public ConnectWalletParameterValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage("Please input password");
        }
    }
}
