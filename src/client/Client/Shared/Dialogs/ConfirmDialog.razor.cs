using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Shared.Dialogs
{
    public partial class ConfirmDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public string ContentTitle { get; set; }

        [Parameter] public string ContentText { get; set; }

        [Parameter] public string ConfirmButtonText { get; set; } = "Confirm";

        [Parameter] public Color ConfirmButtonColor { get; set; } = Color.Primary;

        private void Submit()
        {
            MudDialog.Close(DialogResult.Ok(true));
        }

        private void Cancel() => MudDialog.Cancel();
    }
}
