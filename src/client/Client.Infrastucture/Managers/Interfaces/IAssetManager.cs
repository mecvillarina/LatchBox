using Client.Infrastructure.Models;
using Neo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers.Interfaces
{
    public interface IAssetManager : IManager
    {
        Task<AssetToken> GetTokenAsync(UInt160 assetHash, string address = null);
        Task<List<AssetToken>> GetTokensAsync(string address);
    }
}