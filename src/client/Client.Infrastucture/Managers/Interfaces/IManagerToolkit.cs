using Client.Infrastructure.Models;
using Neo;
using Neo.Network.RPC;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public interface IManagerToolkit : IManager
    {
        NeoSettings NeoSettings { get; }

        RpcClient NeoRpcClient { get; }
        ProtocolSettings NeoProtocolSettings { get; }
        WalletAPI NeoWalletApi { get; }
        Nep17API NeoNep17Api { get; }

        string FilePathRoot { get; }
        string FilePathTemp { get; }
        string FilePathWallet { get; }

        Task SaveWalletAsync(string filename, List<string> addresses);
        Task<WalletInformation> GetWalletAsync();
        Task ClearLocalStorageAsync();
    }
}