using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Models;
using Client.Pages.Modals;
using MudBlazor;
using Neo.SmartContract.Native;

namespace Client.Pages
{
    public partial class AssetsPage
    {
        public bool IsLoaded { get; set; }
        public bool IsCompletelyLoaded { get; set; }
        public List<AssetPageAddressBalance> AddressBalances { get; set; } = new();

        public AssetToken PlatformToken { get; set; }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeAsync(async () =>
                {
                    var addresses = await WalletManager.GetAddressesAsync();
                    AddressBalances = addresses.Select(x => new AssetPageAddressBalance() { Address = x }).ToList();
                    PlatformToken = await PlatformTokenManager.GetTokenAsync();

                    IsLoaded = true;
                    StateHasChanged();
                    await FetchAddressBalancesAsync();
                });
            }
        }

        private async Task FetchAddressBalancesAsync()
        {
            IsCompletelyLoaded = false;

            foreach (var addressBalance in AddressBalances)
            {
                addressBalance.NEOBalanceDisplay = null;
                addressBalance.GASBalanceDisplay = null;
                addressBalance.PlatformTokenBalanceDisplay = null;
            }

            StateHasChanged();

            foreach (var addressBalance in AddressBalances)
            {
                var neo = await AssetManager.GetTokenAsync(NativeContract.NEO.Hash, addressBalance.Address);
                var gas = await AssetManager.GetTokenAsync(NativeContract.GAS.Hash, addressBalance.Address);
                var platformToken = await AssetManager.GetTokenAsync(PlatformToken.AssetScriptHash, addressBalance.Address);
                addressBalance.NEOBalanceDisplay = neo.Balance.ToAmountDisplay(neo.Decimals);
                addressBalance.GASBalanceDisplay = gas.Balance.ToAmountDisplay(gas.Decimals);
                addressBalance.PlatformTokenBalanceDisplay = platformToken.Balance.ToAmountDisplay(platformToken.Decimals);
                StateHasChanged();
            }

            IsCompletelyLoaded = true;
            StateHasChanged();
        }

        private void InvokeAssetsPreviewerModal(string address)
        {
            var options = new DialogOptions() { CloseButton = true };
            var parameters = new DialogParameters()
            {
                 { nameof(AssetsPreviewerModal.Address), address},
            };

            DialogService.Show<AssetsPreviewerModal>("Account", parameters, options);
        }
    }
}