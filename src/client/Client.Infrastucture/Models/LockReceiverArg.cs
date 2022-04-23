using Neo;
using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class LockReceiverArg
    {
        public UInt160 ReceiverAddress { get; set; }
        public BigInteger Amount { get; set; }
    }
}
