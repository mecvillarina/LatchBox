using Client.Infrastructure.Managers.Interfaces;
using FluentValidation;
using Neo.Network.RPC;

namespace Client.Parameters
{
    public class UpsetVestingPeriodParameter
    {
        public Guid Id { get; set; }
        public DateTime? UnlockDate { get; set; }
        public string Name { get; set; }
        public List<AddVestingReceiverParameter> Receivers { get; set; } = new List<AddVestingReceiverParameter>();

    }

    public class AddVestingPeriodParameterValidator : AbstractValidator<UpsetVestingPeriodParameter>
    {
        public AddVestingPeriodParameterValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(v => v.Name)
                .NotNull()
                .NotEmpty();

            RuleFor(v => v.UnlockDate)
               .NotNull()
               .GreaterThan(DateTime.UtcNow);
        }
    }
}
