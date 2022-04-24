using Client.Infrastructure.Settings;
using MudBlazor;
using System;
using System.Threading.Tasks;

namespace Client.Shared
{
    public partial class MainLayout : IAsyncDisposable
    {
        private bool IsAuthenticated { get; set; }
        private bool DrawerOpen { get; set; } = true;
        private MudTheme CurrentTheme { get; set; }
        public string Network { get; set; }
        public string RpcUrl { get; set; }

        protected override void OnInitialized()
        {
            CurrentTheme = ClientPreferenceManager.GetCurrentTheme();
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeAsync(async () =>
                {
                    IsAuthenticated = await AuthManager.IsAuthenticated();
                    await AppBreakpointService.InitAsync();
                    Network = RpcManager.GetNetwork();
                    RpcUrl = RpcManager.GetRpcUrl();
                    StateHasChanged();
                });
            }
        }

        public async ValueTask DisposeAsync()
        {
            await AppBreakpointService.DisposeAsync();
        }
    }
}