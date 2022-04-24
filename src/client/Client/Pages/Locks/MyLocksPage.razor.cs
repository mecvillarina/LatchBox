using Client.Infrastructure.Models;
using Client.Models;
using Client.Pages.Locks.Modals;
using Client.Parameters;
using MudBlazor;
using Neo;
using System.Numerics;

namespace Client.Pages.Locks
{
    public partial class MyLocksPage
    {
        public bool IsLoaded { get; set; }
        public bool IsCompletelyLoaded { get; set; }

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
            IsCompletelyLoaded = false;
            StateHasChanged();

            Locks.Clear();

            var addresses = await WalletManager.GetAddressesAsync();

            foreach (var address in addresses)
            {
                var lockTransactions = await LockTokenVaultManager.GetTransactionsByInitiatorAsync(address);

                foreach (var lockTransaction in lockTransactions)
                {
                    Locks.Add(new LockTransactionInitiatorModel(lockTransaction));
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

            IsCompletelyLoaded = true;
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
            var dialog = DialogService.Show<SearchNep17TokenForLockingModal>($"Search NEP-17 Token");
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                var tokenScriptHash = (UInt160)dialogResult.Data;

                return await AssetManager.GetTokenAsync(tokenScriptHash);
            }

            return null;
        }

        private async Task InvokeRevokeLockModalAsync(LockTransactionInitiatorModel lockModel)
        {
            var lockIndex = lockModel.Transaction.LockIndex;

            var parameters = new DialogParameters();
            parameters.Add(nameof(RevokeLockModal.LockTransaction), lockModel.Transaction);
            parameters.Add(nameof(RevokeLockModal.Model), new RevokeLockParameter() { LockIndex = lockIndex });

            var dialog = DialogService.Show<RevokeLockModal>($"Revoke Lock #{lockIndex}", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                await FetchDataAsync();
            }
        }


        private void InvokeLockPreviewerModal(BigInteger lockIndex)
        {
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium };
            var parameters = new DialogParameters()
            {
                 { nameof(LockPreviewerModal.LockIndex), lockIndex},
            };

            DialogService.Show<LockPreviewerModal>($"Lock #{lockIndex}", parameters, options);
        }
    }
}