using Client.Infrastructure.Models;
using Neo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public interface IWalletManager : IManager
    {
        Task<List<string>> GetAddresses();
        Task<List<WalletAddressAsset>> GetAddressesAssetsAsync();
        Task<List<WalletAddressAsset>> GetAddressAssets(string address, List<UInt160> exceptAssetHashes);
    }
}