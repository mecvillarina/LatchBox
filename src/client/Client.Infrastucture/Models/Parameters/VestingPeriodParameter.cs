using System.Collections.Generic;
using System.Numerics;

namespace Client.Infrastructure.Models.Parameters
{
    public class VestingPeriodParameter
    {
        public string Name { get; set; }
        public BigInteger TotalAmount { get; set; }
        public BigInteger UnlockTime { get; set; }
        public List<VestingReceiverParameter> Receivers { get; set; }
    }
}
