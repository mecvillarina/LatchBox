using MudBlazor;
using System;
using System.Threading.Tasks;

namespace Client.Shared.Components
{
    public partial class AppBarContent : IDisposable
    {
        public string Name { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }

        public void Dispose()
        {
        }

        private async Task LoadDataAsync()
        {
            await InvokeAsync(StateHasChanged);
        }
        private void Logout()
        {
            var parameters = new DialogParameters
            {
                {nameof(Dialogs.Logout.ContentText), "Are you sure you want to logout?"},
                {nameof(Dialogs.Logout.ButtonText), "Logout"},
                {nameof(Dialogs.Logout.Color), Color.Error},
            };

            DialogService.Show<Dialogs.Logout>("Logout", parameters);
        }
    }
}