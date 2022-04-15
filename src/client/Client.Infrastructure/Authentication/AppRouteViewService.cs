using Blazored.LocalStorage;
using Client.Infrastructure.Constants;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Threading.Tasks;

namespace Client.Infrastructure.Authentication
{
    public class AppRouteViewService
    {
        private readonly ILocalStorageService _localStorage;

        public AppRouteViewService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public bool IsAuthenticated { get; private set; }

        public async Task PopulateAsync()
        {
            var walletInfo = await _localStorage.GetItemAsync<string>(StorageConstants.Local.WalletInfo);
            IsAuthenticated = walletInfo == null;
        }
    }
}
