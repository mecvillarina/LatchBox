using Blazored.LocalStorage;
using Client.Infrastructure.Constants;
using Client.Infrastructure.Managers.Interfaces;
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
        public NeoSettings NeoSettings { get; }

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
            NeoSettings = neoSettingsOption.Value;

            NeoProtocolSettings = ProtocolSettings.Load(NeoSettings.ProtocolSettingsConfigFile);
            NeoRpcClient = new RpcClient(new Uri(NeoSettings.RpcUrl), null, null, NeoProtocolSettings);
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

        public async Task ClearLocalStorageAsync()
        {
            await _localStorageService.ClearAsync();
        }
    }
}
