﻿using Client.Infrastructure.Models;
using Neo;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public interface IAssetManager : IManager
    {
        Task<AssetToken> GetPlatformTokenAsync();
        Task<AssetToken> GetTokenAsync(UInt160 assetHash, string address);
    }
}