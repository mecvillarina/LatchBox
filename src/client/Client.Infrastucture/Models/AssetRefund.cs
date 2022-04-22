using Client.Infrastructure.Extensions;
using Neo;
using Neo.VM.Types;
using System.Collections.Generic;
using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class AssetRefund
    {
        public UInt160 TokenScriptHash { get; set; }
        public BigInteger Amount { get; set; }

        public AssetRefund(KeyValuePair<PrimitiveType, StackItem> map, ProtocolSettings protocolSettings)
        {
            TokenScriptHash = Neo.Network.RPC.Utility.GetScriptHash(((ByteString)map.Key).FromByteStringToAccount(), protocolSettings);
            Amount = map.Value.GetInteger();
        }
    }
}
