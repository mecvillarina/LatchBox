using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Pages.Modals;
using Client.Parameters;
using MudBlazor;
using Neo.SmartContract.Native;

namespace Client.Pages.Locks
{
    public partial class MyLocksPage
    {
        public bool IsLoaded { get; set; }
        public bool IsAssetLoaded { get; set; }


        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeAsync(async () =>
                {
                    IsAssetLoaded = true;
                    StateHasChanged();
                });
            }
        }
    }
}