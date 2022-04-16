using Client.Infrastructure.Models;
using MudBlazor;

namespace Client.Pages
{
    public partial class AssetsPage
    {
        public bool IsLoaded { get; set; }
        public List<WalletAddressAsset> AddressAssets { get; set; } = new();

        private TableGroupDefinition<WalletAddressAsset> _groupDefinition = new()
        {
            GroupName = "Address",
            Indentation = false,
            Expandable = true,
            IsInitiallyExpanded = true,
            Selector = (e) => e.Address
        };

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeAsync(async () =>
                {
                    AddressAssets = await WalletManager.GetAddressesAssetsAsync();
                    IsLoaded = true;
                    StateHasChanged();
                    AppDialogService.ShowError(AddressAssets.Count.ToString());
                });
            }
        }

        private async Task OnCopyWalletAddressAsync(string address)
        {
            await ClipboardService.WriteTextAsync(address);
            AppDialogService.ShowSuccess($"{address} copied to clipboard.");
        }
    }
}