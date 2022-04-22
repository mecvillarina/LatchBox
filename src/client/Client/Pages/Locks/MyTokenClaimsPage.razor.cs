using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Models;
using Client.Pages.Modals;
using Client.Parameters;
using MudBlazor;
using Neo.SmartContract.Native;

namespace Client.Pages.Locks
{
    public partial class MyTokenClaimsPage
    {
        public bool IsLoaded { get; set; }

        public List<LockTransactionReceiverModel> Locks { get; set; } = new();

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeAsync(async () =>
                {
                    await FetchDataAsync();
                });
            }
        }

        private async Task FetchDataAsync()
        {
            IsLoaded = false;

            Locks.Clear();

            var addresses = await WalletManager.GetAddressesAsync();

            foreach (var address in addresses)
            {
                var lockTransactions = await LockTokenVaultManager.GetTransactionsByInitiator(address);

                foreach (var lockTransaction in lockTransactions)
                {
                    Locks.Add(new LockTransactionReceiverModel(address, lockTransaction));
                }
            }

            Locks = Locks.OrderByDescending(x => x.Transaction.StartTime).ToList();

            IsLoaded = true;
            StateHasChanged();

            foreach (var @lock in Locks)
            {
                var assetToken = await AssetManager.GetTokenAsync(@lock.Transaction.TokenScriptHash);
                @lock.SetAssetToken(assetToken);
            }

            StateHasChanged();
        }

        private async Task InvokeClaimLockModalAsync(LockTransactionReceiverModel lockModel)
        {
            var lockIndex = lockModel.Transaction.LockIndex;

            var parameters = new DialogParameters();
            parameters.Add(nameof(RevokeLockModal.LockTransaction), lockModel.Transaction);
            parameters.Add(nameof(RevokeLockModal.Model), new RevokeLockParameter() { LockIndex = lockIndex });

            var dialog = DialogService.Show<RevokeLockModal>($"Claim Lock {lockIndex}", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                await FetchDataAsync();
            }
        }
    }
}