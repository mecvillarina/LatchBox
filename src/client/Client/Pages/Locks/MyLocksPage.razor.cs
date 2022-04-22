using Client.Infrastructure.Models;
using Client.Models;
using Client.Pages.Modals;
using Client.Parameters;
using MudBlazor;
using Neo;
using System.Numerics;

namespace Client.Pages.Locks
{
    public partial class MyLocksPage
    {
        public bool IsLoaded { get; set; }

        public List<LockTransactionInitiatorModel> Locks { get; set; } = new();

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
            var lockTransactions = await LockTokenVaultManager.GetTransactionsByInitiator("NVh8ZCYi4rUsvBpMZgCb4gbm3bQVCMafWU");

            foreach (var lockTransaction in lockTransactions)
            {
                Locks.Add(new LockTransactionInitiatorModel(lockTransaction));
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

        private async Task InvokeAddLockModalAsync()
        {
            var assetToken = await InvokeSearchNEP17TokenAsync();

            if (assetToken != null)
            {
                var parameters = new DialogParameters();
                parameters.Add(nameof(AddLockModal.AssetToken), assetToken);

                var dialog = DialogService.Show<AddLockModal>($"Add New Lock", parameters);
                var dialogResult = await dialog.Result;

                if (!dialogResult.Cancelled)
                {
                    await FetchDataAsync();
                }
            }
        }

        private async Task<AssetToken> InvokeSearchNEP17TokenAsync()
        {
            var dialog = DialogService.Show<SearchNep17TokenModal>($"NEP-17 Token");
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                var tokenScriptHash = (UInt160)dialogResult.Data;

                return await AssetManager.GetTokenAsync(tokenScriptHash);
            }

            return null;
        }

        private async Task InvokeRevokeLockModalAsync(LockTransactionInitiatorModel @lock)
        {
            var lockIndex = @lock.Transaction.LockIndex;

            var parameters = new DialogParameters();
            parameters.Add(nameof(RevokeLockModal.LockTransaction), @lock.Transaction);
            parameters.Add(nameof(RevokeLockModal.Model), new RevokeLockParameter() { LockIndex = lockIndex });

            var dialog = DialogService.Show<RevokeLockModal>($"Revoke Lock #{lockIndex}", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                await FetchDataAsync();
            }
        }
    }
}