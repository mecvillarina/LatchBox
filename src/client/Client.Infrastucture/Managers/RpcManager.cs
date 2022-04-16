//using Neo;
//using Neo.Network.RPC;
using System;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public class RpcManager : ManagerBase, IRpcManager
    {
        public RpcManager(IManagerToolkit managerToolkit) : base(managerToolkit)
        {
        }

        public async Task<uint> GetBlock()
        {
            //RpcClient client = new RpcClient(new Uri("http://seed1t4.neo.org:20332"), null, null, ProtocolSettings.Load("config.json"));
            //uint blockCount = await client.GetBlockCountAsync().ConfigureAwait(false);
            //return blockCount;
            return 0;
        }
    }
}
