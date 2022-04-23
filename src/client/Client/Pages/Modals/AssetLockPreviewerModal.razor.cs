using Client.Infrastructure.Models;
using Client.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Neo;
using System.Numerics;

namespace Client.Pages.Modals
{
    public partial class AssetLockPreviewerModal
    {
        [Parameter] public AssetCounterModel AssetCounterModel { get; set; }
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        public bool IsLoaded { get; set; }

        public AssetToken AssetToken { get; set; }
        public List<LockTransactionModel> Transactions { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeAsync(async () =>
                {
                    AssetToken = AssetCounterModel.AssetToken;
                    await FetchDataAsync();
                });
            }
        }


        private async Task FetchDataAsync()
        {
            IsLoaded = false;
            StateHasChanged();

            Transactions = new();
            
            var lockTransactions = await LockTokenVaultManager.GetTransactionsByAssetAsync(AssetToken.AssetScriptHash);

            foreach(var lockTransaction in lockTransactions)
            {
                Transactions.Add(new LockTransactionModel(lockTransaction, AssetToken));
            }

            IsLoaded = true;
            StateHasChanged();
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