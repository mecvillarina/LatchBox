using Neo;
using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class LockReceiverParameter
    {
        public UInt160 ReceiverAddress { get; set; }
        public BigInteger Amount { get; set; }
    }
}
