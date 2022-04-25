using Client.Infrastructure.Models;
using Client.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Numerics;

namespace Client.Pages.Vestings.Modals
{
    public partial class VestingPreviewerModal
    {
        [Parameter] public BigInteger VestingIndex { get; set; }
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        public bool IsLoaded { get; set; }
        public VestingTransactionModel Model { get; set; }
        public string ShareLink { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
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
            try
            {
                var transaction = await VestingTokenVaultManager.GetTransactionAsync(VestingIndex);
                var assetToken = await AssetManager.GetTokenAsync(transaction.TokenScriptHash);

                Model = new VestingTransactionModel(transaction, assetToken);
                ShareLink = $"{NavigationManager.BaseUri}view/vestings/{VestingIndex}";
                IsLoaded = true;
                MudDialog.SetTitle("");
                StateHasChanged();
            }
            catch
            {
                AppDialogService.ShowError("LatchBox Vesting not found.");
                MudDialog.Cancel();
            }
        }

        private void InvokeVestingPeriodPreviewerModal(VestingPeriod period)
        {
            var receivers = Model.Transaction.Receivers.Where(x => x.PeriodIndex == period.PeriodIndex).ToList();
            
            var parameters = new DialogParameters();
            parameters.Add(nameof(VestingPeriodPreviewerModal.AssetToken), Model.AssetToken);
            parameters.Add(nameof(VestingPeriodPreviewerModal.Period), period);
            parameters.Add(nameof(VestingPeriodPreviewerModal.Receivers), receivers);
            parameters.Add(nameof(VestingPeriodPreviewerModal.Transaction), Model.Transaction);

            var options = new DialogOptions() { CloseButton = true };
            DialogService.Show<VestingPeriodPreviewerModal>($"Vesting Period", parameters, options);
        }

        private async Task OnCopyShareLinkAsync()
        {
            await ClipboardService.WriteTextAsync(ShareLink);
            AppDialogService.ShowSuccess("Vesting Link copied to clipboard.");
        }
    }
}