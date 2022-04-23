using Client.Infrastructure.Models;
using Client.Models;

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
    }
}