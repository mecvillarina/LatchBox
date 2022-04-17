using Client.Infrastructure.Managers.Interfaces;
using System.Collections.Generic;
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
    }
}
