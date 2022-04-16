using Client.Infrastructure.Settings;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Client.Shared
{
    public partial class AuthLayout : IAsyncDisposable
    {
        public string MainContainerClass { get; set; }
        public MudTheme CurrentTheme { get; set; }

        protected override void OnInitialized()
        {
            CurrentTheme = ClientPreferenceManager.GetCurrentTheme();
        }

        public async ValueTask DisposeAsync()
        {
            AppBreakpointService.BreakpointChanged -= HandleBreakpointChanged;
            await AppBreakpointService.DisposeAsync();
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                AppBreakpointService.BreakpointChanged += HandleBreakpointChanged;
                await AppBreakpointService.InitAsync();
                await SetStyles(AppBreakpointService.CurrentBreakpoint);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public async void HandleBreakpointChanged(object sender, Breakpoint breakpoint) => await SetStyles(breakpoint);

        private async Task SetStyles(Breakpoint breakpoint)
        {
            if (breakpoint == Breakpoint.Xs)
            {
                MainContainerClass = "pa-0 ma-0 d-flex align-stretch justify-center background-dark";
            }
            else
            {
                MainContainerClass = "pa-0 ma-0 d-flex align-center justify-center background-dark";
            }

            await InvokeAsync(StateHasChanged);
        }
    }
}
