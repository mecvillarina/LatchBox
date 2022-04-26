using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Models;
using Client.Pages.Locks.Modals;
using Client.Parameters;
using MudBlazor;
using Neo.SmartContract.Native;
using System.Numerics;

namespace Client.Pages.Locks
{
    public partial class MyLockClaimsPage
    {
        public bool IsLoaded { get; set; }
        public bool IsCompletelyLoaded { get; set; }

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
            IsCompletelyLoaded = false;
            StateHasChanged();

            Locks.Clear();

            var addresses = await WalletManager.GetAddressesAsync();

            foreach (var address in addresses)
            {
                var lockTransactions = await LockTokenVaultManager.GetTransactionsByReceiverAsync(address);

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

            IsCompletelyLoaded = true;
            StateHasChanged();
        }

        private async Task InvokeClaimLockModalAsync(LockTransactionReceiverModel lockModel)
        {
            var lockIndex = lockModel.Transaction.LockIndex;

            var parameters = new DialogParameters();
            parameters.Add(nameof(ClaimLockModal.Model), new ClaimLockParameter()
            {
                LockIndex = lockIndex,
                ReceiverAddress = lockModel.Receiver.ReceiverAddress,
                ReceiverHash160 = lockModel.Receiver.ReceiverHash160,
                AmountDisplay = lockModel.AmountDisplay
            });

            var dialog = DialogService.Show<ClaimLockModal>($"Claim from Lock #{lockIndex} as {lockModel.Receiver.ReceiverAddress.ToMask(6)}", parameters);
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

        private async Task OnTextToClipboardAsync(string text)
        {
            await ClipboardService.WriteTextAsync(text);
            AppDialogService.ShowSuccess("Contract ScriptHash copied to clipboard.");
        }

    }
}