//using Neo;
//using Neo.Network.RPC;
using Client.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Neo;
using Neo.Network.RPC;
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
            uint blockCount = await ManagerToolkit.NeoRpcClient.GetBlockCountAsync().ConfigureAwait(false);
            return blockCount;
        }
    }
}
