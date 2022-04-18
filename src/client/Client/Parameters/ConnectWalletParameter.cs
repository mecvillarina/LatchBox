using FluentValidation;

namespace Client.Parameters
{
    public class ConnectWalletParameter
    {
        public string Password { get; set; } = string.Empty;
    }

    public class LoginParameterValidator : AbstractValidator<ConnectWalletParameter>
    {
        public LoginParameterValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage("Please input password");
        }
    }
}
