using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Numerics;

namespace Client.Pages.Modals
{
    public partial class LockPreviewerModal : IDisposable
    {
        [Parameter] public BigInteger LockIndex { get; set; }
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        public bool IsLoaded { get; set; }
        public LockTransactionModel Model { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeAsync(async () =>
                {
                    AppBreakpointService.BreakpointChanged += AppBreakpointService_BreakpointChanged;
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
                
                IsLoaded = true;
                StateHasChanged();
            }
            catch
            {
                MudDialog.Cancel();
            }
        }


        private void AppBreakpointService_BreakpointChanged(object sender, Breakpoint e)
        {
            StateHasChanged();
        }


        public void Dispose()
        {
            AppBreakpointService.BreakpointChanged -= AppBreakpointService_BreakpointChanged;
        }
    }
}