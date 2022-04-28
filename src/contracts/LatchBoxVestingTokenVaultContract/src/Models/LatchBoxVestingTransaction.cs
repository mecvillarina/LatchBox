using System.Numerics;

namespace LatchBoxVestingTokenVaultContract
{
    public class LatchBoxVestingTransaction
    {
        public BigInteger Idx;
        public LatchBoxVesting Vesting;
        public LatchBoxVestingPeriod[] Periods;
        public LatchBoxVestingReceiver[] Receivers;
    }
}