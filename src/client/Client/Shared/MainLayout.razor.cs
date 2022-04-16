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

        protected override void OnInitialized()
        {
            CurrentTheme = ClientPreferenceManager.GetCurrentTheme();
            //FetchDataExecutor.StartExecuting();
            //RenderUIExecutor.StartExecuting();
        }

        private async Task LoadDataAsync()
        {
            //IsAuthenticated = AppRouteViewService.IsAuthenticated;
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //await AppBreakpointService.InitAsync();
                //await LoadDataAsync();
                //StateHasChanged();

                if (!IsAuthenticated)
                {
                    NavigationManager.NavigateTo("/", true);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            //await AppBreakpointService.DisposeAsync();
        }
    }
}