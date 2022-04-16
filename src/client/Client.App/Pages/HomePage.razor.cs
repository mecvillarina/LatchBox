using Blazored.LocalStorage;
using Client.Infrastructure.Managers;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Threading.Tasks;

namespace Client.App.Pages
{
    public partial class HomePage
    {
        [Inject] public IRpcManager _rpcManager { get; set; }
        protected override void OnInitialized()
        {
            AppBreakpointService.BreakpointChanged += async (s, e) => await SetStylesAsync(e);
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await SetStylesAsync(AppBreakpointService.CurrentBreakpoint);
                Console.WriteLine(await _rpcManager.GetBlock());
            }
        }

        private async Task SetStylesAsync(Breakpoint e)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}