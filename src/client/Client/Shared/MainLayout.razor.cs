using Client.Infrastructure.Settings;
using MudBlazor;
using System;
using System.Threading.Tasks;

namespace Client.Shared
{
    public partial class MainLayout : IAsyncDisposable
    {
        private bool IsAuthVerified { get; set; }
        private bool DrawerOpen { get; set; } = true;
        private MudTheme CurrentTheme { get; set; }

        protected override void OnInitialized()
        {
            CurrentTheme = ClientPreferenceManager.GetCurrentTheme();
        }

        private async Task<bool> CheckAuthorization()
        {
            var isAuthenticated = await AuthManager.IsAuthenticated();

            if (!isAuthenticated)
            {
                NavigationManager.NavigateTo("/auth/login", true);
            }

            return isAuthenticated;
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var isAuthenticated = await CheckAuthorization();

                if (isAuthenticated)
                {
                    await AppBreakpointService.InitAsync();
                    IsAuthVerified = true;
                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            await AppBreakpointService.DisposeAsync();
        }
    }
}