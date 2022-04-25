using Client.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Numerics;

namespace Client.Pages.Locks.Modals
{
    public partial class LockPreviewerModal
    {
        [Parameter] public BigInteger LockIndex { get; set; }
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        public bool IsLoaded { get; set; }
        public LockTransactionModel Model { get; set; }
        public string ShareLink { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
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
            try
            {
                var transaction = await LockTokenVaultManager.GetTransactionAsync(LockIndex);
                var assetToken = await AssetManager.GetTokenAsync(transaction.TokenScriptHash);

                Model = new LockTransactionModel(transaction, assetToken);
                ShareLink = $"{NavigationManager.BaseUri}view/locks/{LockIndex}";
                IsLoaded = true;
                MudDialog.SetTitle("");
                StateHasChanged();
            }
            catch
            {
                AppDialogService.ShowError("LatchBox Lock not found.");
                MudDialog.Cancel();
            }
        }

        private async Task OnCopyShareLinkAsync()
        {
            await ClipboardService.WriteTextAsync(ShareLink);
            AppDialogService.ShowSuccess("Lock Link copied to clipboard.");
        }
    }
}