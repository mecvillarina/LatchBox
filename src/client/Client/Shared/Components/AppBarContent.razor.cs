using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Pages.Modals;
using Client.Parameters;
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

        private void InvokeBuyPlatformTokenModal(string currency)
        {
            var options = new DialogOptions() { MaxWidth = MaxWidth.Small };
            var parameters = new DialogParameters();

            if (currency == "NEO")
            {
                parameters.Add(nameof(BuyPlatformTokenModal.Model), new BuyPlatformTokenParameter() { Currency = "NEO", CurrencyDecimals = NativeContract.NEO.Decimals, CurrencyHash = NativeContract.NEO.Hash });
                parameters.Add(nameof(BuyPlatformTokenModal.BuyConversationDisplay), $"1 NEO ≈ {PlatformTokensPerNEO} {PlatformTokenSymbol}");
            }
            else if (currency == "GAS")
            {
                parameters.Add(nameof(BuyPlatformTokenModal.Model), new BuyPlatformTokenParameter() { Currency = "GAS", CurrencyDecimals = NativeContract.GAS.Decimals, CurrencyHash = NativeContract.GAS.Hash });
                parameters.Add(nameof(BuyPlatformTokenModal.BuyConversationDisplay), $"1 GAS ≈ {PlatformTokensPerGAS} {PlatformTokenSymbol}");
            }

            if (parameters.Any())
            {
                DialogService.Show<BuyPlatformTokenModal>($"Buy {PlatformTokenSymbol}", parameters, options);
            }
        }
        private void InvokeDisconnectWalletDialog()
        {
            var parameters = new DialogParameters
            {
                {nameof(DisconnectWalletDialog.ContentText), "Are you sure you want to disconnect your wallet?"},
                {nameof(DisconnectWalletDialog.ButtonText), "Disconnect"},
                {nameof(DisconnectWalletDialog.Color), Color.Error},
            };

            DialogService.Show<DisconnectWalletDialog>("Logout", parameters);
        }
    }
}