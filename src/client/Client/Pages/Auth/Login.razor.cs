using Blazored.FluentValidation;
using Client.Infrastructure.Managers;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Client.Pages.Auth
{
    public partial class Login
    {
        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        private LoginParameter Model { get; set; } = new();
        public bool IsProcessing { get; set; }

        private string WalletFilename = "Please select file location";
        private IBrowserFile WalletBrowserFile;
        private bool PasswordVisibility;
        private InputType PasswordInput = InputType.Password;
        private string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;

        private async Task SubmitAsync()
        {
            if (Validated)
            {
                if (WalletBrowserFile == null)
                {
                    AppDialogService.ShowError("Please select wallet file path");
                    return;
                }

                IsProcessing = true;

                await AuthManager.Login(WalletBrowserFile, Model.Password);
                var isAuthenticated = await AuthManager.IsAuthenticated();

                if (isAuthenticated)
                {
                    await Task.Delay(1000);
                    NavigationManager.NavigateTo("/", true);
                    return;
                }
                else
                {
                    AppDialogService.ShowError("Open wallet error, please check wallet file or password and retry");
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

        private async Task OnWalletFileChanged(InputFileChangeEventArgs e)
        {
            foreach (var file in e.GetMultipleFiles(1))
            {
                if (file.Name.EndsWith(".json"))
                {
                    WalletBrowserFile = file;
                    WalletFilename = file.Name;
                }
                else
                {
                    AppDialogService.ShowError("Selected wallet file is not supported. NEP6 wallet filename is .json.");
                }
            }
        }
    }
}