using Neo;
using System;
using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class VestingReceiver
    {
        public BigInteger PeriodIndex { get; set; }
        public string Name { get; set; }
        public UInt160 ReceiverHash160 { get; set; }
        public string ReceiverAddress { get; set; }
        public BigInteger Amount { get; set; }
        public DateTimeOffset? DateClaimed { get; set; }
        public DateTimeOffset? DateRevoked { get; set; }
    }
}
