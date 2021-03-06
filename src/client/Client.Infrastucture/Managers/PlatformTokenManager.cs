using Client.Infrastructure.Managers.Interfaces;
using Client.Infrastructure.Models;
using Neo;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.Wallets;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public class PlatformTokenManager : ManagerBase, IPlatformTokenManager
    {
        public PlatformTokenManager(IManagerToolkit managerToolkit) : base(managerToolkit)
        {
        }

        public UInt160 TokenScriptHash => Neo.Network.RPC.Utility.GetScriptHash(ManagerToolkit.NeoSettings.PlatformTokenHash, ProtocolSettings.Default);

        public async Task<AssetToken> GetTokenAsync()
        {
            var tokenInfo = await ManagerToolkit.NeoNep17Api.GetTokenInfoAsync(TokenScriptHash).ConfigureAwait(false);

            return new AssetToken()
            {
                Name = tokenInfo.Name,
                Symbol = tokenInfo.Symbol,
                Decimals = tokenInfo.Decimals,
                TotalSupply = tokenInfo.TotalSupply,
                AssetScriptHash = TokenScriptHash,
            };
        }

        public async Task<PlatformTokenSaleInfo> GetSaleInfoAsync()
        {
            var tokenInfo = await GetTokenAsync();
            byte[] script = Neo.Helper.Concat(
                TokenScriptHash.MakeScript("isTokenSaleEnabled"),
                TokenScriptHash.MakeScript("tokensPerNEO"),
                TokenScriptHash.MakeScript("tokensPerGAS"));
            var result = await ManagerToolkit.NeoRpcClient.InvokeScriptAsync(script).ConfigureAwait(false);
            var stack = result.Stack;

            return new PlatformTokenSaleInfo
            {
                IsTokenOnSale = stack[0].GetBoolean(),
                TokensPerNEO = Convert.ToDecimal(((double)stack[1].GetInteger()) * Math.Pow(10, NativeContract.NEO.Decimals) / Math.Pow(10, tokenInfo.Decimals)),
                TokensPerGAS = Convert.ToDecimal(((double)stack[2].GetInteger()) * Math.Pow(10, NativeContract.GAS.Decimals) / Math.Pow(10, tokenInfo.Decimals))
            };
        }

        public async Task<PlatformTokenStats> GetTokenStatsAsync()
        {
            byte[] script = Neo.Helper.Concat(TokenScriptHash.MakeScript("maxSupply"));
            var result = await ManagerToolkit.NeoRpcClient.InvokeScriptAsync(script).ConfigureAwait(false);
            var stack = result.Stack;

            var token = await GetTokenAsync();
            var stats = new PlatformTokenStats()
            {
                AssetScriptHash = token.AssetScriptHash,
                Decimals = token.Decimals,
                TotalSupply = token.TotalSupply,
                Name = token.Name,
                Symbol = token.Symbol
            };

            stats.MaxSupply = stack[0].GetInteger();
            return stats;
        }

        public async Task<bool> IsTokenSaleEnabled()
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(TokenScriptHash, "isTokenSaleEnabled").ConfigureAwait(false);
            return result.Stack.Single().GetBoolean();
        }

        public async Task<BigInteger> GetTokensPerNEO()
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(TokenScriptHash, "tokensPerNEO").ConfigureAwait(false);
            return result.Stack.Single().GetInteger();
        }

        public async Task<BigInteger> GetTokensPerGAS()
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(TokenScriptHash, "tokensPerGAS").ConfigureAwait(false);
            return result.Stack.Single().GetInteger();
        }

        public async Task<bool> BuyPlatformTokenAsync(UInt160 scriptHash, KeyPair fromKey, BigInteger amount)
        {
            var tx = await ManagerToolkit.NeoWalletApi.TransferAsync(scriptHash, fromKey, TokenScriptHash, amount, null);

            var rpcTx = await ManagerToolkit.NeoWalletApi.WaitTransactionAsync(tx);
            var appLog = await ManagerToolkit.NeoRpcClient.GetApplicationLogAsync(tx.Hash.ToString());

            var exec = appLog.Executions.Single();
            return exec.VMState == Neo.VM.VMState.HALT && exec.Stack.Single().GetBoolean();
        }
    }
}
