using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Models;
using Client.Pages.Modals;
using Client.Parameters;
using MudBlazor;
using Neo.SmartContract.Native;
using System.Numerics;

namespace Client.Pages.Vestings
{
    public partial class MyVestingClaimsPage
    {
        public bool IsLoaded { get; set; }
        public bool IsCompletelyLoaded { get; set; }

        public List<VestingTransactionReceiverModel> Vestings { get; set; } = new();

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

            Vestings.Clear();

            var addresses = await WalletManager.GetAddressesAsync();

            foreach (var address in addresses)
            {
                var transactions = await VestingTokenVaultManager.GetTransactionsByReceiverAsync(address);

                foreach (var transaction in transactions)
                {
                    var receivers = transaction.Receivers.Where(x => x.ReceiverAddress == address);
                    foreach (var receiver in receivers)
                    {
                        Vestings.Add(new VestingTransactionReceiverModel(transaction, receiver));
                    }
                }
            }

            Vestings = Vestings.OrderBy(x => x.Period.UnlockTime).Select(x => new
            {
                IsPastTime = x.Period.UnlockTime > DateTime.UtcNow,
                Vesting = x
            }).OrderBy(x => x.IsPastTime).Select(x => x.Vesting).ToList();

            IsLoaded = true;
            StateHasChanged();

            foreach (var @lock in Vestings)
            {
                var assetToken = await AssetManager.GetTokenAsync(@lock.Transaction.TokenScriptHash);
                @lock.SetAssetToken(assetToken);
            }

            IsCompletelyLoaded = true;
            StateHasChanged();
        }

        //private async Task InvokeClaimLockModalAsync(LockTransactionReceiverModel lockModel)
        //{
        //    var lockIndex = lockModel.Transaction.LockIndex;

        //    var parameters = new DialogParameters();
        //    parameters.Add(nameof(ClaimLockModal.Model), new ClaimLockParameter()
        //    {
        //        LockIndex = lockIndex,
        //        ReceiverAddress = lockModel.Receiver.ReceiverAddress,
        //        ReceiverHash160 = lockModel.Receiver.ReceiverHash160,
        //        AmountDisplay = lockModel.AmountDisplay
        //    });

        //    var dialog = DialogService.Show<ClaimLockModal>($"Claim from Lock #{lockIndex} as {lockModel.Receiver.ReceiverAddress.ToMask(6)}", parameters);
        //    var dialogResult = await dialog.Result;

        //    if (!dialogResult.Cancelled)
        //    {
        //        await FetchDataAsync();
        //    }
        //}

        //private void InvokeLockPreviewerModal(BigInteger lockIndex)
        //{
        //    var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium };
        //    var parameters = new DialogParameters()
        //    {
        //         { nameof(LockPreviewerModal.LockIndex), lockIndex},
        //    };

        //    DialogService.Show<LockPreviewerModal>($"Lock #{lockIndex}", parameters, options);
        //}
    }
}