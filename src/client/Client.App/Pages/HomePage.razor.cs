using MudBlazor;
using System.Threading.Tasks;

namespace Client.App.Pages
{
    public partial class HomePage
    {
        protected override void OnInitialized()
        {
            AppBreakpointService.BreakpointChanged += async (s, e) => await SetStylesAsync(e);
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await SetStylesAsync(AppBreakpointService.CurrentBreakpoint);
            }
        }

        private async Task SetStylesAsync(Breakpoint e)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}