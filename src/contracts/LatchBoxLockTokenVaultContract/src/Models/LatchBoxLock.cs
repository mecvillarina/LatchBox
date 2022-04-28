using System.Numerics;

using Neo;

namespace LatchBoxLockTokenVaultContract
{
    public class LatchBoxLock
    {
        public UInt160 TokenScriptHash;
        public UInt160 InitiatorAddress;
        public BigInteger CreationTime;
        public BigInteger StartTime;
        public BigInteger UnlockTime;
        public bool IsRevocable;
        public bool IsRevoked;
        public bool IsActive;
    }
}