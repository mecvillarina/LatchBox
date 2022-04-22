using Client.Models;
using Client.Pages.Modals;
using MudBlazor;

namespace Client.Pages.Locks
{
    public partial class MyTokenRefundsPage
    {
        public bool IsLoaded { get; set; }

        public List<AssetRefundModel> Refunds { get; set; } = new();

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeAsync(async () =>
                {
                    await FetchDataAsync();
                });
            }
        }

        private async Task FetchDataAsync()
        {
            IsLoaded = false;

            Refunds.Clear();

            var addresses = await WalletManager.GetAddressesAsync();

            foreach (var address in addresses)
            {
                var assetRefunds = await LockTokenVaultManager.GetRefundsAsync(address);

                foreach (var assetRefund in assetRefunds)
                {
                    Refunds.Add(new AssetRefundModel(address, assetRefund));
                }
            }

            IsLoaded = true;
            StateHasChanged();

            foreach (var refund in Refunds)
            {
                var assetToken = await AssetManager.GetTokenAsync(refund.AssetRefund.TokenScriptHash);
                refund.SetAssetToken(assetToken);
            }

            StateHasChanged();
        }


        private async Task InvokeClaimRefundModalAsync(AssetRefundModel model)
        {
            var parameters = new DialogParameters();
            parameters.Add(nameof(ClaimRefundModal.Model), model);

            var dialog = DialogService.Show<ClaimRefundModal>($"Claim Refund for {model.AssetToken.Name} ({model.AssetToken.Symbol})", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                await FetchDataAsync();
            }
        }

    }
}