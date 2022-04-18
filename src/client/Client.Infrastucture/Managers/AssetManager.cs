using Client.Infrastructure.Extensions;
using Client.Infrastructure.Managers.Interfaces;
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

        public async Task<AssetToken> GetTokenAsync(UInt160 assetHash, string address = null)
        {
            var tokenInfo = await ManagerToolkit.NeoNep17Api.GetTokenInfoAsync(assetHash).ConfigureAwait(false);

            decimal balance = 0;

            if (!string.IsNullOrEmpty(address))
            {
                var bal = await ManagerToolkit.NeoWalletApi.GetTokenBalanceAsync(assetHash.ToString(), address).ConfigureAwait(false);
                balance = bal.ToAmount(tokenInfo.Decimals);
            }

            return new AssetToken()
            {
                Name = tokenInfo.Name,
                Symbol = tokenInfo.Symbol,
                Decimals = tokenInfo.Decimals,
                TotalSupply = tokenInfo.TotalSupply,
                AssetHash = assetHash,
                Balance = balance,
            };

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
