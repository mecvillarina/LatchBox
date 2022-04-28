using System.Numerics;

using Neo;

namespace LatchBoxVestingTokenVaultContract
{
    public class LatchBoxVesting
    {
        public UInt160 TokenScriptHash;
        public UInt160 InitiatorAddress;
        public BigInteger CreationTime;
        public bool IsRevocable;
        public bool IsRevoked;
        public bool IsActive;
    }
}