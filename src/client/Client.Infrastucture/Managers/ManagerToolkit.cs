﻿using Blazored.LocalStorage;
using Client.Infrastructure.Constants;
using Client.Infrastructure.Models;
using Microsoft.Extensions.Options;
using Neo;
using Neo.Network.RPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public class ManagerToolkit : IManagerToolkit
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly NeoSettings _neoSettings;

        public RpcClient NeoRpcClient { get; }
        public ProtocolSettings NeoProtocolSettings { get; }
        public WalletAPI NeoWalletApi { get; }
        public Nep17API NeoNep17Api { get; }

        public string FilePathRoot => Directory.GetCurrentDirectory();
        public string FilePathTemp => Path.Combine(Directory.GetCurrentDirectory(), "..", "temp");
        public string FilePathWallet => Path.Combine(Directory.GetCurrentDirectory(), "..", "wallet");

        public ManagerToolkit(ILocalStorageService localStorageService, IOptions<NeoSettings> neoSettingsOption)
        {
            _localStorageService = localStorageService;
            _neoSettings = neoSettingsOption.Value;

            NeoProtocolSettings = ProtocolSettings.Load(_neoSettings.ProtocolSettingsConfigFile);
            NeoRpcClient = new RpcClient(new Uri(_neoSettings.RpcUrl), null, null, NeoProtocolSettings);
            NeoWalletApi = new WalletAPI(NeoRpcClient);
            NeoNep17Api = new Nep17API(NeoRpcClient);
            Init();
        }

        private void Init()
        {
            if (!Directory.Exists(FilePathTemp))
            {
                Directory.CreateDirectory(FilePathTemp);
            }

            if (!Directory.Exists(FilePathWallet))
            {
                Directory.CreateDirectory(FilePathWallet);
            }
        }

        public async Task SaveWalletAsync(string filename, List<string> addresses)
        {
            var wallet = new WalletInformation()
            {
                Filename = filename,
                Addresses = addresses
            };

            await _localStorageService.SetItemAsync(StorageConstants.Local.Wallet, wallet);
        }

        public async Task<WalletInformation> GetWalletAsync()
        {
            return await _localStorageService.GetItemAsync<WalletInformation>(StorageConstants.Local.Wallet);
        }

        public async Task<PlatformToken> GetPlatformTokenAsync()
        {
            var tokenHash = _neoSettings.PlatformTokenHash;
            var scriptHash = Neo.Network.RPC.Utility.GetScriptHash(tokenHash, ProtocolSettings.Default);
            var symbol = await NeoNep17Api.SymbolAsync(scriptHash).ConfigureAwait(false);
            var decimals = await NeoNep17Api.DecimalsAsync(scriptHash).ConfigureAwait(false);

            return new PlatformToken()
            {
                AssetHash = scriptHash,
                Symbol = symbol,
                Decimals = decimals,
            };
        }
    }
}
