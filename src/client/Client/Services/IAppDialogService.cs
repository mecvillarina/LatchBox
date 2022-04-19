using Neo.Wallets;

namespace Client.Services
{
    public interface IAppDialogService
    {
        void ShowSuccess(string message);
        void ShowWarning(string message);
        void ShowError(string message);
        void ShowErrors(List<string> messages);
        Task<KeyPair> ShowConfirmWalletTransaction(string walletAddress);
    }
}