using MudBlazor;
using System.Collections.Generic;

namespace Client.App.Services
{
    public class AppDialogService : IAppDialogService
    {
        private readonly ISnackbar _snackbar;

        public AppDialogService(ISnackbar snackBar)
        {
            _snackbar = snackBar;
        }

        public void ShowSuccess(string message)
        {
            _snackbar.Add(message, Severity.Success);
        }

        public void ShowWarning(string message)
        {
            _snackbar.Add(message, Severity.Warning);
        }

        public void ShowError(string message)
        {
            _snackbar.Add(message, Severity.Error);
        }

        public void ShowErrors(List<string> messages)
        {
            foreach(var message in messages)
            {
                _snackbar.Add(message, Severity.Error);
            }
        }
    }
}
