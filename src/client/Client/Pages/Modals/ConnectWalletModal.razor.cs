using Blazored.FluentValidation;
using Client.Infrastructure.Exceptions;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Client.Pages.Modals
{
    public partial class ConnectWalletModal
    {
        private ConnectWalletParameter Model { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }
        private string WalletFilename = "Please select file location";
        private IBrowserFile WalletBrowserFile;
        private bool PasswordVisibility;
        private InputType PasswordInput = InputType.Password;
        private string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
        public string RpcUrl { get; set; }
        public string Network { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                Network = RpcManager.GetNetwork();
                RpcUrl = RpcManager.GetRpcUrl();
                StateHasChanged();
            }
        }

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
                    NavigationManager.NavigateTo("/", true);
                    return;
                }
                else
                {
                    Model.Password = string.Empty;
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

        private void OnWalletFileChanged(InputFileChangeEventArgs e)
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

        public void Cancel()
        {
            MudDialog.Cancel();
        }

    }
}