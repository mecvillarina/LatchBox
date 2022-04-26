using Client.Models;
using Client.Pages.Vestings.Modals;
using MudBlazor;

namespace Client.Pages.Vestings
{
    public partial class MyVestingRefundsPage
    {
        public bool IsLoaded { get; set; }
        public bool IsCompletelyLoaded { get; set; }

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
            IsCompletelyLoaded = false;
            StateHasChanged();

            Refunds.Clear();
            var addresses = await WalletManager.GetAddressesAsync();

            foreach (var address in addresses)
            {
                var assetRefunds = await VestingTokenVaultManager.GetRefundsAsync(address);

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

            IsCompletelyLoaded = true;
            StateHasChanged();
        }


        private async Task InvokeClaimRefundModalAsync(AssetRefundModel model)
        {
            var parameters = new DialogParameters();
            parameters.Add(nameof(ClaimVestingRefundModal.Model), model);

            var dialog = DialogService.Show<ClaimVestingRefundModal>($"Claim Refund for {model.AssetToken.Name} ({model.AssetToken.Symbol})", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                await FetchDataAsync();
            }
        }

        private async Task OnTextToClipboardAsync(string text)
        {
            await ClipboardService.WriteTextAsync(text);
            AppDialogService.ShowSuccess("Contract ScriptHash copied to clipboard.");
        }

    }
}