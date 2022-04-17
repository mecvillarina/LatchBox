using Client.Infrastructure.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Pages.Modals
{
    public partial class AssetsPreviewerModal
    {
        [Parameter] public string Address { get; set; }
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        public bool IsProcessing { get; set; }
        public bool IsLoaded { get; set; }

        public List<AssetToken> Assets { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeAsync(async () =>
                {
                    await FetchData();
                    IsLoaded = true;
                    StateHasChanged();
                });
            }
        }

        private async Task FetchData()
        {
            Assets = await AssetManager.GetTokensAsync(Address);

        }
    }
}