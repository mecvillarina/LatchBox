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

        //public async Task<List<WalletAddressAsset>> GetAddressAssets(string address)
        //{
        //    var assets = await ManagerToolkit.NeoRpcClient.GetNep17BalancesAsync(address).ConfigureAwait(false);

        //    var addressAssets = new List<WalletAddressAsset>();

        //    foreach (var assetBalance in assets.Balances)
        //    {
        //        var symbol = await ManagerToolkit.NeoNep17Api.SymbolAsync(assetBalance.AssetHash).ConfigureAwait(false);
        //        var decimals = await ManagerToolkit.NeoNep17Api.DecimalsAsync(assetBalance.AssetHash).ConfigureAwait(false);

        //        var balance = Convert.ToDecimal((double)(assetBalance.Amount) / Math.Pow(10, (int)decimals));

        //        addressAssets.Add(new WalletAddressAsset()
        //        {
        //            Address = address,
        //            AssetHash = assetBalance.AssetHash,
        //            Balance = balance,
        //            Symbol = symbol,
        //            Decimals = decimals
        //        });
        //    }

        //    return addressAssets;
        //}
    }
}
