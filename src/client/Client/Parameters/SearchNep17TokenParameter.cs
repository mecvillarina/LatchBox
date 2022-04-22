using Client.Infrastructure.Managers.Interfaces;
using FluentValidation;
using Neo.Network.RPC;

namespace Client.Parameters
{
    public class SearchNep17TokenParameter
    {
        public string TokenScriptHash { get; set; }
    }

    public class SearchNep17TokenParameterValidator : AbstractValidator<SearchNep17TokenParameter>
    {
        public SearchNep17TokenParameterValidator(IManagerToolkit managerToolkit)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(v => v.TokenScriptHash)
                .NotNull().WithMessage("'Token ScriptHash' must not be empty.")
                .NotEmpty().WithMessage("'Token ScriptHash' must not be empty.")
                .Must(x =>
                {
                    try
                    {
                        Utility.GetScriptHash(x, managerToolkit.NeoProtocolSettings);
                        return x.Length == 42;
                    }
                    catch
                    {
                        return false;
                    }
                }).WithMessage("Invalid 'Token ScriptHash' format.");
        }
    }
}
