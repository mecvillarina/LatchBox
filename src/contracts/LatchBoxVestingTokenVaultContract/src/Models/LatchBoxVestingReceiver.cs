using System.Numerics;

using Neo;

namespace LatchBoxVestingTokenVaultContract
{
    public class LatchBoxVestingReceiver
    {
        public BigInteger PeriodIndex;
        public string Name;
        public UInt160 Address;
        public BigInteger Amount; 
        public BigInteger DateClaimed;
        public BigInteger DateRevoked;
    }
}