using Client.Infrastructure.Managers.Interfaces;
using FluentValidation;
using Neo.Network.RPC;

namespace Client.Parameters
{
    public class AddVestingParameter
    {
        public string WalletAddress { get; set; }
        public string TokenScriptHash { get; set; }
        public bool IsRevocable { get; set; }
        public List<UpsetVestingPeriodParameter> Periods { get; set; } = new List<UpsetVestingPeriodParameter>();
    }

}
