using Client.Infrastructure.Extensions;
using Neo;
using Neo.VM.Types;
using System.Collections.Generic;
using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class AssetCounter
    {
        public UInt160 TokenScriptHash { get; private set; }
        public BigInteger LockedAmount { get; private set; }
        public BigInteger UnlockedAmount { get; private set; }

        public AssetCounter(KeyValuePair<PrimitiveType, StackItem> map, ProtocolSettings protocolSettings)
        {
            TokenScriptHash = Neo.Network.RPC.Utility.GetScriptHash(((ByteString)map.Key).FromByteStringToAccount(), protocolSettings);
            var value = (Array)map.Value;
            LockedAmount = value[0].GetInteger();
            UnlockedAmount = value[1].GetInteger();
        }
    }
}
