using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Pages.Modals;
using Client.Shared.Dialogs;
using MudBlazor;
using Neo.SmartContract.Native;

namespace Client.Shared.Components
{
    public partial class AppBarContent
    {
        public bool IsLoaded { get; set; }
        public bool IsPlatformTokenLoaded { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Network { get; set; }
        public string RpcUrl { get; set; }
        
        public AssetToken PlatformToken { get; set; }
        public string PlatformTokenSymbol { get; set; }

        public bool IsPlatformTokenOnSale { get; set; }
        public string PlatformTokensPerNEO { get; set; }
        public string PlatformTokensPerGAS { get; set; }
        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeAsync(async () =>
                {
                    IsAuthenticated = await AuthManager.IsAuthenticated();
                    Network = RpcManager.GetNetwork();
                    RpcUrl = RpcManager.GetRpcUrl();
                    IsLoaded = true;
                    StateHasChanged();

                    PlatformToken = await PlatformTokenManager.GetTokenAsync();
                    PlatformTokenSymbol = PlatformToken.Symbol;

                    var platformTokenSaleInfo = await PlatformTokenManager.GetSaleInfoAsync();
                    IsPlatformTokenOnSale = platformTokenSaleInfo.IsTokenOnSale;
                    PlatformTokensPerNEO = $"{platformTokenSaleInfo.TokensPerNEO.ToAmountDisplay(PlatformToken.Decimals)}";
                    PlatformTokensPerGAS = $"{platformTokenSaleInfo.TokensPerGAS.ToAmountDisplay(PlatformToken.Decimals)}";
                    IsPlatformTokenLoaded = true;
                    StateHasChanged();
                });
            }
        }

        private void InvokeConnectWalletModal()
        {
            var options = new DialogOptions() { MaxWidth = MaxWidth.ExtraSmall };
            DialogService.Show<ConnectWalletModal>("Connect NEP6 Wallet", options);
        }

        private void Logout()
        {
            var parameters = new DialogParameters
            {
                {nameof(LogoutDialog.ContentText), "Are you sure you want to logout?"},
                {nameof(LogoutDialog.ButtonText), "Logout"},
                {nameof(LogoutDialog.Color), Color.Error},
            };

            DialogService.Show<LogoutDialog>("Logout", parameters);
        }
    }
}