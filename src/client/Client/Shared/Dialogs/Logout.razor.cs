using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Shared.Dialogs
{
    public partial class Logout
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public string ContentText { get; set; }

        [Parameter] public string ButtonText { get; set; }

        [Parameter] public Color Color { get; set; }

        private async Task Submit()
        {
            //await AccountManager.LogoutAsync();
            NavigationManager.NavigateTo("/auth/login");
            MudDialog.Close(DialogResult.Ok(true));
        }

        private void Cancel() => MudDialog.Cancel();
    }
}
