using System.Collections.Generic;
using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class VestingPeriodParameter
    {
        public string Name { get; set; }
        public BigInteger TotalAmount { get; set; }
        public BigInteger DurationInDays { get; set; }
        public List<VestingReceiverParameter> Receivers { get; set; }
    }
}
