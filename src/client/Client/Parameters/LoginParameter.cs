﻿using FluentValidation;

namespace Client.Parameters
{
    public class LoginParameter
    {
        public string Password { get; set; } = string.Empty;
    }

    public class LoginParameterValidator : AbstractValidator<LoginParameter>
    {
        public LoginParameterValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage("Please input password");
        }
    }
}
