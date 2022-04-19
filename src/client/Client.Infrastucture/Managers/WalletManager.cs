using Client.Infrastructure.Managers.Interfaces;
using Client.Infrastructure.Models;
using Neo.Wallets;
using Neo.Wallets.NEP6;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public class WalletManager : ManagerBase, IWalletManager
    {
        public WalletManager(IManagerToolkit managerToolkit) : base(managerToolkit)
        {
        }

        public async Task<List<string>> GetAddressesAsync()
        {
            var wallet = await ManagerToolkit.GetWalletAsync();

            if (wallet != null)
            {
                return wallet.Addresses;
            }

            return new List<string>();
        }

        public async Task<List<WalletAccountKeyPair>> GetWalletAccountsAsync(string password)
        {
            try
            {
                var wallet = await ManagerToolkit.GetWalletAsync();

                if(wallet != null)
                {
                    var walletFilePath = $"{ManagerToolkit.FilePathWallet}/{wallet.Filename}";

                    NEP6Wallet nep6Wallet = new NEP6Wallet(walletFilePath, ManagerToolkit.NeoProtocolSettings);
                    using (nep6Wallet.Unlock(password))
                    {
                        return nep6Wallet.GetAccounts().Select(x => new WalletAccountKeyPair() { Account = x, KeyPair = x.GetKey() }).ToList();
                    }
                }
            }
            catch
            {
                // do nothing
            }

            return new List<WalletAccountKeyPair>();
        }

        public async Task<WalletAccountKeyPair> GetWalletAccountAsync(string address, string password)
        {
            try
            {
                var wallet = await ManagerToolkit.GetWalletAsync();

                if (wallet != null)
                {
                    var walletFilePath = $"{ManagerToolkit.FilePathWallet}/{wallet.Filename}";

                    NEP6Wallet nep6Wallet = new NEP6Wallet(walletFilePath, ManagerToolkit.NeoProtocolSettings);
                    using (nep6Wallet.Unlock(password))
                    {
                        var account = nep6Wallet.GetAccounts().First(x => x.Address == address);
                        return new WalletAccountKeyPair() { Account = account, KeyPair = account.GetKey() };
                    }
                }
            }
            catch
            {
                // do nothing
            }

            return null;
        }
    }
}
