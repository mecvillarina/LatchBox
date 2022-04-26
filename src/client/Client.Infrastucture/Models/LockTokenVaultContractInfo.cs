using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class LockTokenVaultContractInfo
    {
        public BigInteger TotalLocks { get; set; }
        public BigInteger BurnedAmount { get; set; }
    }
}
