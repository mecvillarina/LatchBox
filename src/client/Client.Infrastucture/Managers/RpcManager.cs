using Client.Infrastructure.Managers.Interfaces;
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

        public string GetRpcUrl()
        {
            return ManagerToolkit.NeoSettings.RpcUrl;
        }

        public string GetNetwork()
        {
            return ManagerToolkit.NeoSettings.Network;
        }
    }
}
