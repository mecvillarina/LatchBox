using Client.Infrastructure.Models;
using Neo;
using Neo.Network.P2P.Payloads;
using Neo.Wallets;
using System.Numerics;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers.Interfaces
{
    public interface IPlatformTokenManager : IManager
    {
        UInt160 TokenScriptHash { get; }
        Task<AssetToken> GetTokenAsync();
        Task<PlatformTokenSaleInfo> GetSaleInfoAsync();

        Task<bool> IsTokenSaleEnabled();
        Task<BigInteger> GetTokensPerNEO();
        Task<BigInteger> GetTokensPerGAS();
        Task<bool> BuyPlatformTokenAsync(UInt160 scriptHash, KeyPair fromKey, BigInteger amount);
    }
}