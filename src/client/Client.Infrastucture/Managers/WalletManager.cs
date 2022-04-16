using Blazored.LocalStorage;
using Client.Infrastructure.Models;
using Neo;
using Neo.SmartContract.Native;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public class WalletManager : ManagerBase, IWalletManager
    {
        private readonly ILocalStorageService _localStorageService;
        public WalletManager(IManagerToolkit managerToolkit, ILocalStorageService localStorageService) : base(managerToolkit)
        {
            _localStorageService = localStorageService;
        }

        public async Task<List<string>> GetAddresses()
        {
            var wallet = await ManagerToolkit.GetWalletAsync();

            if (wallet != null)
            {
                return wallet.Addresses;
            }

            return new List<string>();
        }

        public async Task<List<WalletAddressAssets>> GetAddressesAssetsAsync()
        {
            var addresses = await GetAddresses();
            var addressesAssets = new List<WalletAddressAssets>();
            foreach (var address in addresses)
            {
                var addressAssets = await GetAddressAssets(address).ConfigureAwait(false);
                addressesAssets.Add(addressAssets);
            }

            return addressesAssets;
        }

        public async Task<WalletAddressAssets> GetAddressAssets(string address)
        {
            var platformToken = await ManagerToolkit.GetPlatformTokenAsync().ConfigureAwait(false);

            var assets = await ManagerToolkit.NeoRpcClient.GetNep17BalancesAsync(address).ConfigureAwait(false);

            var walletAddress = new WalletAddressAssets()
            {
                Address = address,
                Assets = new List<AssetToken>()
            };

            var mainAssetHashes = new List<UInt160>() { NativeContract.GAS.Hash, NativeContract.NEO.Hash, platformToken.AssetHash };

            foreach (var assetBalance in assets.Balances)
            {
                if (!mainAssetHashes.Any(x => x == assetBalance.AssetHash))
                {
                    var symbol = await ManagerToolkit.NeoNep17Api.SymbolAsync(assetBalance.AssetHash).ConfigureAwait(false);
                    var decimals = await ManagerToolkit.NeoNep17Api.DecimalsAsync(assetBalance.AssetHash).ConfigureAwait(false);

                    walletAddress.Assets.Add(new AssetToken()
                    {
                        AssetHash = assetBalance.AssetHash,
                        Balance = assetBalance.Amount,
                        Symbol = symbol,
                        Decimals = decimals
                    });
                }
            }

            walletAddress.Assets.Insert(0, new AssetToken()
            {
                AssetHash = NativeContract.GAS.Hash,
                Balance = await ManagerToolkit.NeoWalletApi.GetTokenBalanceAsync(NativeContract.GAS.Hash.ToString(), address).ConfigureAwait(false),
                Symbol = NativeContract.GAS.Symbol,
                Decimals = NativeContract.GAS.Decimals
            });

            walletAddress.Assets.Insert(0, new AssetToken()
            {
                AssetHash = NativeContract.NEO.Hash,
                Balance = await ManagerToolkit.NeoWalletApi.GetTokenBalanceAsync(NativeContract.NEO.Hash.ToString(), address).ConfigureAwait(false),
                Symbol = NativeContract.NEO.Symbol,
                Decimals = NativeContract.NEO.Decimals
            });

            walletAddress.Assets.Insert(0, new AssetToken()
            {
                AssetHash = platformToken.AssetHash,
                Balance = await ManagerToolkit.NeoWalletApi.GetTokenBalanceAsync(platformToken.AssetHash.ToString(), address).ConfigureAwait(false),
                Symbol = platformToken.Symbol,
                Decimals = platformToken.Decimals
            });

            return walletAddress;
        }


    }
}
