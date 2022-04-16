using Blazored.FluentValidation;
using Client.Parameters;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Auth
{
    public partial class Login
    {
        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        private LoginParameter Model { get; set; } = new();
        public bool IsProcessing { get; set; }

        private async Task SubmitAsync()
        {
            if (Validated)
            {
                IsProcessing = true;
                string rootpath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory());
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
    }
}