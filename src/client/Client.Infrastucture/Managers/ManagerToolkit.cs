using Blazored.LocalStorage;
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
        private readonly ILocalStorageService _localStorage;
        public RpcClient NeoRpcClient { get; }
        public ProtocolSettings NeoProtocolSettings { get; }
        public string FilePathRoot => Directory.GetCurrentDirectory();
        public string FilePathTemp => Path.Combine(Directory.GetCurrentDirectory(), "..", "temp");
        public string FilePathWallet => Path.Combine(Directory.GetCurrentDirectory(), "..", "wallet");

        public ManagerToolkit(ILocalStorageService localStorage, IOptions<NeoSettings> neoSettingsOption)
        {
            _localStorage = localStorage;

            NeoProtocolSettings = ProtocolSettings.Load(neoSettingsOption.Value.ProtocolSettingsConfigFile);
            NeoRpcClient = new RpcClient(new Uri(neoSettingsOption.Value.RpcUrl), null, null, NeoProtocolSettings);

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

            await _localStorage.SetItemAsync("Wallet", wallet);
        }

        public async Task<WalletInformation> GetWalletAsync()
        {
            return await _localStorage.GetItemAsync<WalletInformation>("Wallet");
        }
    }
}
