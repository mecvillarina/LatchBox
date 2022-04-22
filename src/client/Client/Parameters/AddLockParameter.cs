using Client.Infrastructure.Managers.Interfaces;
using FluentValidation;
using Neo.Network.RPC;

namespace Client.Parameters
{
    public class AddLockParameter
    {
        public string WalletAddress { get; set; }
        public string TokenScriptHash { get; set; }
        public DateTime? UnlockDate { get; set; }
        public bool IsRevocable { get; set; }
        public List<AddLockReceiverParameter> Receivers { get; set; } = new List<AddLockReceiverParameter>();
    }

}
