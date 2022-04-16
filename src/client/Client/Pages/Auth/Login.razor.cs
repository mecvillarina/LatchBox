using Blazored.FluentValidation;
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
        private Stream WalletFileStream;
        private bool PasswordVisibility;
        private InputType PasswordInput = InputType.Password;
        private string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;

        private async Task SubmitAsync()
        {
            if (Validated)
            {
                if(WalletFileStream == null)
                {
                    AppDialogService.ShowError("Please select wallet file path");
                    return;
                }

                IsProcessing = true;
                string rootpath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", "temp");

                if (!Directory.Exists(rootpath))
                {
                    Directory.CreateDirectory(rootpath);
                }

                rootpath = @$"{rootpath}\{Guid.NewGuid().ToString()}.json";
                File.WriteAllText(rootpath, "Hello World");
                bool? result = await DialogService.ShowMessageBox("Path", rootpath, yesText: "Ok");

                try
                {
                    //await Task.Delay(1000);
                    //NavigationManager.NavigateTo("/", true);
                }
                catch (Exception ex)
                {
                    AppDialogService.ShowError(ex.Message);
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
            long maxFileSize = 1024 * 1024 * 1;

            foreach (var file in e.GetMultipleFiles(1))
            {
                if (file.Name.EndsWith(".json"))
                {
                    WalletFileStream = file.OpenReadStream(maxFileSize);
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