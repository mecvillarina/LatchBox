using Client.Infrastructure.Models;
using Neo.Wallets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers.Interfaces
{
    public interface IWalletManager : IManager
    {
        Task<List<string>> GetAddressesAsync();
        Task<WalletAccountKeyPair> GetWalletAccountAsync(string address, string password);
        Task<List<WalletAccountKeyPair>> GetWalletAccountsAsync(string password);
    }
}