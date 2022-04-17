using Client.Infrastructure.Models;
using Neo;
using Neo.SmartContract.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public class AssetManager : ManagerBase, IAssetManager
    {
        public AssetManager(IManagerToolkit managerToolkit) : base(managerToolkit)
        {
        }

        public async Task<AssetToken> GetPlatformTokenAsync()
        {
            var tokenHash = ManagerToolkit.NeoSettings.PlatformTokenHash;
            var scriptHash = Neo.Network.RPC.Utility.GetScriptHash(tokenHash, ProtocolSettings.Default);
            var symbol = await ManagerToolkit.NeoNep17Api.SymbolAsync(scriptHash).ConfigureAwait(false);
            var decimals = await ManagerToolkit.NeoNep17Api.DecimalsAsync(scriptHash).ConfigureAwait(false);

            return new AssetToken()
            {
                AssetHash = scriptHash,
                Symbol = symbol,
                Decimals = decimals,
            };
        }

        public async Task<AssetToken> GetTokenAsync(UInt160 assetHash, string address)
        {
            if (assetHash == NativeContract.NEO.Hash)
            {
                return new AssetToken()
                {
                    AssetHash = NativeContract.NEO.Hash,
                    Balance = await ManagerToolkit.NeoWalletApi.GetNeoBalanceAsync(address).ConfigureAwait(false),
                    Symbol = NativeContract.NEO.Symbol,
                    Decimals = NativeContract.NEO.Decimals
                };
            }
            else if (assetHash == NativeContract.GAS.Hash)
            {
                return new AssetToken()
                {
                    AssetHash = NativeContract.GAS.Hash,
                    Balance = await ManagerToolkit.NeoWalletApi.GetGasBalanceAsync(address).ConfigureAwait(false),
                    Symbol = NativeContract.GAS.Symbol,
                    Decimals = NativeContract.GAS.Decimals
                };
            }
            else
            {
                var symbol = await ManagerToolkit.NeoNep17Api.SymbolAsync(assetHash).ConfigureAwait(false);
                var decimals = await ManagerToolkit.NeoNep17Api.DecimalsAsync(assetHash).ConfigureAwait(false);
                var balance = Convert.ToDecimal(((double)(await ManagerToolkit.NeoWalletApi.GetTokenBalanceAsync(assetHash.ToString(), address).ConfigureAwait(false))) / Math.Pow(10, (int)decimals));

                return new AssetToken()
                {
                    AssetHash = assetHash,
                    Symbol = symbol,
                    Decimals = decimals,
                    Balance = balance,
                };
            }
        }

        public async Task<List<AssetToken>> GetTokensAsync(string address)
        {
            var assetBalances = await ManagerToolkit.NeoRpcClient.GetNep17BalancesAsync(address).ConfigureAwait(false);
            var assets = new List<AssetToken>();

            foreach (var assetBalance in assetBalances.Balances)
            {
                var asset = await GetTokenAsync(assetBalance.AssetHash, address).ConfigureAwait(false);
                assets.Add(asset);
            }

            assets = assets.OrderBy(x => x.Symbol).ToList();
            return assets;
        }
    }
}
