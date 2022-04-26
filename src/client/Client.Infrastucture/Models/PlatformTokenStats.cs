using Neo.Network.RPC.Models;
using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class PlatformTokenStats : AssetToken
    {
        public BigInteger MaxSupply { get; set; }
    }
}
