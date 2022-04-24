using System;
using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class VestingPeriod
    {
        public string Name { get; set; }
        public BigInteger PeriodIndex { get; set; }
        public BigInteger TotalAmount { get; set; }
        public DateTimeOffset UnlockTime { get; set; }
    }
}
