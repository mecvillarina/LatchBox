using System.Numerics;

namespace LatchBoxLockTokenVaultContract
{
    public class LatchBoxLockTransaction
    {
        public BigInteger Idx;
        public LatchBoxLock Lock;
        public LatchBoxLockReceiver[] Receivers;
    }
}