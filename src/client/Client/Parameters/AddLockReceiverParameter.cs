using Client.Infrastructure.Managers.Interfaces;
using FluentValidation;
using Neo.Network.RPC;

namespace Client.Parameters
{
    public class AddLockReceiverParameter
    {
        public Guid Id { get; set; }
        public string ReceiverAddress { get; set; }
        public double Amount { get; set; }
    }

    public class AddLockReceiverParameterValidator : AbstractValidator<AddLockReceiverParameter>
    {
        public AddLockReceiverParameterValidator(IManagerToolkit managerToolkit)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(v => v.ReceiverAddress)
                .NotNull().WithMessage("'Receiver Address' must not be empty.")
                .NotEmpty().WithMessage("'Receiver Address' must not be empty.")
                .Must(x =>
                {
                    try
                    {
                        Utility.GetScriptHash(x, managerToolkit.NeoProtocolSettings);
                        return x.Length == 34;
                    }
                    catch
                    {
                        return false;
                    }
                }).WithMessage("Invalid 'Receiver Address' format.");

            RuleFor(v => v.Amount)
                //.NotEmpty().WithMessage("'Amount' must not be empty and decimal places should be in dot format.")
                .GreaterThan(0.0).WithMessage("'Amount' must be greater than 0");
        }
    }
}
