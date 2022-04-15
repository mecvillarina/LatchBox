using Client.Infrastructure.Settings;
using MudBlazor;
using System;
using System.Threading.Tasks;

namespace Client.App.Shared
{
    public partial class MainLayout : IAsyncDisposable
    {
        private bool IsAuthenticated { get; set; }
        private bool DrawerOpen { get; set; } = true;
        private MudTheme CurrentTheme { get; set; }

        protected override void OnInitialized()
        {
            CurrentTheme = _clientPreferenceManager.GetCurrentTheme();
            FetchDataExecutor.StartExecuting();
            RenderUIExecutor.StartExecuting();
        }

        protected async override Task OnInitializedAsync()
        {
            await AppBreakpointService.InitAsync();
        }

        private async Task LoadDataAsync()
        {
            IsAuthenticated = AppRouteViewService.IsAuthenticated;
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadDataAsync();
                StateHasChanged();

                if (!IsAuthenticated)
                {
                    _navigationManager.NavigateTo("/", true);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            await AppBreakpointService.DisposeAsync();
        }
    }
}