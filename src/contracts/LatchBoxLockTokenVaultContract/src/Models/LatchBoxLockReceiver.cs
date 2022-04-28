using System.Numerics;

using Neo;

namespace LatchBoxLockTokenVaultContract
{
    public class LatchBoxLockReceiver
    {
        public UInt160 ReceiverAddress;
        public BigInteger Amount;
        public BigInteger DateClaimed;
        public BigInteger DateRevoked;
        public bool IsActive;
    }
}