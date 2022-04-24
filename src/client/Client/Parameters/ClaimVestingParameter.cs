using Neo;
using System.Numerics;

namespace Client.Parameters
{
    public class ClaimVestingParameter
    {
        public string AmountDisplay { get; set; }
        public BigInteger VestingIdx { get; set; }
        public BigInteger PeriodIdx { get; set; }
        public string PeriodName { get; set; }
        public UInt160 ReceiverHash160 { get; set; }
        public string ReceiverAddress { get; set; }
    }
}
