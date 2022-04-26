using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class VestingTokenVaultContractInfo
    {
        public BigInteger TotalVestings { get; set; }
        public BigInteger BurnedAmount { get; set; }
    }
}
