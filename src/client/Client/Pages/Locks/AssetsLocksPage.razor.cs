using Client.Infrastructure.Models;
using Client.Models;
using Client.Pages.Modals;
using MudBlazor;

namespace Client.Pages.Locks
{
    public partial class AssetsLocksPage
    {
        public bool IsLoaded { get; set; }

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
    }
}