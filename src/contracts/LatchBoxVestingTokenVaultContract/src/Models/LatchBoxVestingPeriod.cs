using System.Numerics;

namespace LatchBoxVestingTokenVaultContract
{
    public class LatchBoxVestingPeriod
    {
        public BigInteger PeriodIndex;
        public string Name;
        public BigInteger TotalAmount;
        public BigInteger UnlockTime;
    }
}