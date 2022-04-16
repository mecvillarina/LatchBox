using Client.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public interface IWalletManager : IManager
    {
        Task<List<string>> GetAddresses();
        Task<List<WalletAddressAssets>> GetAddressesAssetsAsync();
        Task<WalletAddressAssets> GetAddressAssets(string address);
    }
}