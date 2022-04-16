using Neo;
using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class AssetToken
    {
        public UInt160 AssetHash { get; set; }
        public string Symbol { get; set; }
        public BigInteger Balance { get; set; }
        public byte Decimals { get; set; }
    }
}
