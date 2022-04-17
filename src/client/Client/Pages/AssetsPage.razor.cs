using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Pages.Modals;
using Client.Parameters;
using MudBlazor;
using Neo.SmartContract.Native;

namespace Client.Pages
{
    public partial class AssetsPage
    {
        public bool IsLoaded { get; set; }
        public bool IsBalanceLoaded { get; set; }
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
                    PlatformToken = await AssetManager.GetPlatformTokenAsync();
                    
                    IsLoaded = true;
                    StateHasChanged();
                    foreach (var addressBalance in AddressBalances)
                    {
                        var neo = await AssetManager.GetTokenAsync(NativeContract.NEO.Hash, addressBalance.Address);
                        var gas = await AssetManager.GetTokenAsync(NativeContract.GAS.Hash, addressBalance.Address);
                        var platformToken = await AssetManager.GetTokenAsync(PlatformToken.AssetHash, addressBalance.Address);
                        addressBalance.NEOBalanceDisplay = neo.Balance.ToBalanceDisplay(neo.Decimals);
                        addressBalance.GASBalanceDisplay = gas.Balance.ToBalanceDisplay(gas.Decimals);
                        addressBalance.PlatformTokenBalanceDisplay = platformToken.Balance.ToBalanceDisplay(platformToken.Decimals); 
                    }


                    IsBalanceLoaded = true;
                    StateHasChanged();
                });
            }
        }

        private async Task OnCopyWalletAddressAsync(string address)
        {
            await ClipboardService.WriteTextAsync(address);
            AppDialogService.ShowSuccess($"{address} copied to clipboard.");
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