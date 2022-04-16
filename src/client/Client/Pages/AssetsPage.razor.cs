namespace Client.Pages
{
    public partial class AssetsPage
    {
        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var addressesAssets = await WalletManager.GetAddressesAssetsAsync();

                AppDialogService.ShowError(addressesAssets.Count.ToString());
            }
        }
    }
}