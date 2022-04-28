using System.Numerics;

using Neo;

namespace LatchBoxVestingTokenVaultContract
{
    public struct LatchBoxVestingReceiverParameter
    {
        public string Name;
        public UInt160 Address;
        public BigInteger Amount; 
    }
}