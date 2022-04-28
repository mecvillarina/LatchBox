using System.Numerics;

namespace LatchBoxVestingTokenVaultContract
{
    public struct LatchBoxVestingPeriodParameter
    {
        public string Name;
        public BigInteger TotalAmount;
        public BigInteger UnlockTime;
        public LatchBoxVestingReceiverParameter[] Receivers;
    }
}