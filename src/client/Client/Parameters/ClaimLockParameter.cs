using Neo;
using System.Numerics;

namespace Client.Parameters
{
    public class ClaimLockParameter
    {
        public string AmountDisplay { get; set; }
        public BigInteger LockIndex { get; set; }
        public UInt160 ReceiverHash160 { get; set; }
        public string ReceiverAddress { get; set; }
    }
}
