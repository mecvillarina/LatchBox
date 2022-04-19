using Blazored.FluentValidation;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Pages.Modals
{
    public partial class ConfirmWalletTransactionModal
    {
        [Parameter] public ConfirmWalletTransactionParameter Model { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }

        public bool PasswordVisibility { get; set; }
        public InputType PasswordInput { get; set; } = InputType.Password;
        public string PasswordInputIcon { get; set; } = Icons.Material.Filled.VisibilityOff;

        private async Task SubmitAsync()
        {
            if (Validated)
            {
                IsProcessing = true;

                var walletAccount = await WalletManager.GetWalletAccountAsync(Model.WalletAddress, Model.Password);

                if (walletAccount != null)
                {
                    MudDialog.Close(DialogResult.Ok(walletAccount));
                    return;
                }
                else
                {
                    AppDialogService.ShowError("Open wallet error, please check password and retry");
                }

                IsProcessing = false;
            }
        }

        private void TogglePasswordVisibility()
        {
            if (PasswordVisibility)
            {
                PasswordVisibility = false;
                PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
                PasswordInput = InputType.Password;
            }
            else
            {
                PasswordVisibility = true;
                PasswordInputIcon = Icons.Material.Filled.Visibility;
                PasswordInput = InputType.Text;
            }
        }

        public void Cancel()
        {
            MudDialog.Cancel();
        }

    }
}