using Neo;
using System.Numerics;

namespace Client.Infrastructure.Models.Parameters
{
    public class VestingReceiverParameter
    {
        public string Name { get; set; }
        public UInt160 Address { get; set; }
        public BigInteger Amount { get; set; }
    }
}
