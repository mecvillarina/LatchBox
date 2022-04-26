using Client.Infrastructure.Models;
using Client.Models;
using Client.Pages.Locks.Modals;
using MudBlazor;

namespace Client.Pages.Locks
{
    public partial class AssetsLocksPage
    {
        public bool IsLoaded { get; set; }
        public bool IsCompletelyLoaded { get; set; }

        public List<AssetCounterModel> AssetCounters { get; set; } = new();

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
            AssetCounters.Clear();

            var assetCounters = await LockTokenVaultManager.GetAssetsCounterAsync();

            foreach (var assetCounter in assetCounters)
            {
                AssetCounters.Add(new AssetCounterModel(assetCounter));
            }

            IsLoaded = true;
            StateHasChanged();

            foreach (var assetCounter in AssetCounters)
            {
                var assetToken = await AssetManager.GetTokenAsync(assetCounter.AssetCounter.TokenScriptHash);
                assetCounter.SetAssetToken(assetToken);
            }

            AssetCounters = AssetCounters.OrderBy(x => x.AssetToken.Name).ToList();
            IsCompletelyLoaded = true;
            StateHasChanged();
        }

        private void InvokeAssetLockPreviewerModal(AssetCounterModel model)
        {
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium };
            var parameters = new DialogParameters()
            {
                 { nameof(AssetLockPreviewerModal.AssetCounterModel), model},
            };

            DialogService.Show<AssetLockPreviewerModal>($"Asset Locks of {model.AssetToken.Symbol}", parameters, options);
        }

        private async Task OnTextToClipboardAsync(string text)
        {
            await ClipboardService.WriteTextAsync(text);
            AppDialogService.ShowSuccess("Contract ScriptHash copied to clipboard.");
        }
    }
}