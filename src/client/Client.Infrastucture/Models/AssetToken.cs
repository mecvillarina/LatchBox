using Neo;
using Neo.Network.RPC.Models;
using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class AssetToken : RpcNep17TokenInfo
    {
        public UInt160 AssetScriptHash { get; set; }
        public decimal Balance { get; set; }
    }
}
