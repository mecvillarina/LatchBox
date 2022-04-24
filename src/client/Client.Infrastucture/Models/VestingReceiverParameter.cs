using Neo;
using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class VestingReceiverParameter
    {
        public string Name { get; set; }
        public UInt160 Address { get; set; }
        public BigInteger Amount { get; set; }
        public BigInteger DateClaimed { get; set; }
        public BigInteger DateRevoked { get; set; }
    }
}
