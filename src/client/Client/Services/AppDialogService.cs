using Client.Infrastructure.Models;
using Client.Parameters;
using Client.Shared.Dialogs;
using MudBlazor;
using Neo.Wallets;

namespace Client.Services
{
    public class AppDialogService : IAppDialogService
    {
        private readonly ISnackbar _snackbar;
        private readonly IDialogService _dialogService;
        public AppDialogService(ISnackbar snackBar, IDialogService dialogService)
        {
            _snackbar = snackBar;
            _dialogService = dialogService;
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
            foreach (var message in messages)
            {
                _snackbar.Add(message, Severity.Error);
            }
        }

        public async Task<KeyPair> ShowConfirmWalletTransaction(string walletAddress)
        {
            var options = new DialogOptions() { MaxWidth = MaxWidth.ExtraSmall, Position = DialogPosition.TopRight };
            var parameters = new DialogParameters();
            parameters.Add(nameof(ConfirmWalletTransactionDialog.Model), new ConfirmWalletTransactionParameter() { WalletAddress = walletAddress });
            var dialog = _dialogService.Show<ConfirmWalletTransactionDialog>($"Confirm Wallet Transaction", parameters, options);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                return ((WalletAccountKeyPair)dialogResult.Data).KeyPair;
            }

            return null;
        }

    }
}
