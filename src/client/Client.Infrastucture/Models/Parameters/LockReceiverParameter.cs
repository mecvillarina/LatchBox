using Neo;
using System.Numerics;

namespace Client.Infrastructure.Models.Parameters
{
    public class LockReceiverParameter
    {
        public UInt160 ReceiverAddress { get; set; }
        public BigInteger Amount { get; set; }
    }
}
