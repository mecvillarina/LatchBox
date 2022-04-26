using Client.Infrastructure.Extensions;
using Client.Models;
using Client.Pages.Vestings.Modals;
using Client.Parameters;
using MudBlazor;
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

            Vestings = Vestings.OrderBy(x => x.Transaction.IsRevoked).ThenBy(x => x.Period.UnlockTime).Select(x => new
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

        private async Task InvokeClaimVestingModalAsync(VestingTransactionReceiverModel vestingModel)
        {
            var vestingIdx = vestingModel.Transaction.VestingIndex;
            var periodName = vestingModel.Period.Name;

            var parameters = new DialogParameters();
            parameters.Add(nameof(ClaimVestingModal.Model), new ClaimVestingParameter()
            {
                VestingIdx = vestingIdx,
                PeriodIdx = vestingModel.Period.PeriodIndex,
                PeriodName = periodName,
                ReceiverAddress = vestingModel.Receiver.ReceiverAddress,
                ReceiverHash160 = vestingModel.Receiver.ReceiverHash160,
                AmountDisplay = vestingModel.AmountDisplay
            });

            var dialog = DialogService.Show<ClaimVestingModal>($"Claim from Vesting #{vestingIdx} - {periodName} as {vestingModel.Receiver.ReceiverAddress.ToMask(6)}", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                await FetchDataAsync();
            }
        }

        private void InvokeVestingPreviewerModal(BigInteger vestingIndex)
        {
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium };
            var parameters = new DialogParameters()
            {
                 { nameof(VestingPreviewerModal.VestingIndex), vestingIndex},
            };

            DialogService.Show<VestingPreviewerModal>($"Vesting #{vestingIndex}", parameters, options);
        }

        private async Task OnTextToClipboardAsync(string text)
        {
            await ClipboardService.WriteTextAsync(text);
            AppDialogService.ShowSuccess("Contract ScriptHash copied to clipboard.");
        }
    }
}