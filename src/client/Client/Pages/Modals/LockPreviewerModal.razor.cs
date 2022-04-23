using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Numerics;

namespace Client.Pages.Modals
{
    public partial class LockPreviewerModal : IDisposable
    {
        [Parameter] public BigInteger LockIndex { get; set; }
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        public bool IsLoaded { get; set; }

        public LockTransaction LockTransaction { get; set; }
        public AssetToken AssetToken { get; set; }

        public string InitiatorAddressDisplay { get; set; }
        public string TotalAmountDisplay { get; set; }
        public string DateStartDisplay { get; set; }
        public string DateUnlockDisplay { get; set; }
        public string RevocableDisplay { get; set; }
        public string StatusDisplay { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeAsync(async () =>
                {
                    AppBreakpointService.BreakpointChanged += AppBreakpointService_BreakpointChanged;
                    await FetchDataAsync();
                });
            }
        }

        private async Task FetchDataAsync()
        {
            try
            {
                LockTransaction = await LockTokenVaultManager.GetTransactionAsync(LockIndex);
                AssetToken = await AssetManager.GetTokenAsync(LockTransaction.TokenScriptHash);

                BigInteger totalAmount = 0;
                foreach (var receiver in LockTransaction.Receivers)
                {
                    totalAmount += receiver.Amount;
                }

                InitiatorAddressDisplay = LockTransaction.InitiatorAddress;
                TotalAmountDisplay = $"{totalAmount.ToAmount(AssetToken.Decimals).ToAmountDisplay(AssetToken.Decimals)} {AssetToken.Symbol}";
                DateStartDisplay = LockTransaction.StartTime.ToCurrentTimeZone().ToFormat("MMMM dd, yyyy hh:mm tt");
                DateUnlockDisplay = LockTransaction.UnlockTime.ToCurrentTimeZone().ToFormat("MMMM dd, yyyy hh:mm tt");
                RevocableDisplay = LockTransaction.IsRevocable ? "Yes" : "No";

                if (LockTransaction.IsActive)
                {
                    if (DateTime.UtcNow < LockTransaction.UnlockTime)
                    {
                        StatusDisplay = "Locked";
                    }
                    else
                    {
                        StatusDisplay = "Unlocked";
                    }
                }
                else if (LockTransaction.IsRevoked)
                {
                    StatusDisplay = "Revoked";
                }
                else
                {
                    StatusDisplay = "Claimed";
                }

                SetStyles();
                IsLoaded = true;
                StateHasChanged();
            }
            catch
            {
                MudDialog.Cancel();
            }
        }


        private void AppBreakpointService_BreakpointChanged(object sender, Breakpoint e)
        {
            SetStyles();
            StateHasChanged();
        }

        private void SetStyles()
        {
            if (LockTransaction != null)
            {
                if (AppBreakpointService.CurrentBreakpoint == Breakpoint.Xs)
                {
                    InitiatorAddressDisplay = LockTransaction.InitiatorAddress.ToMask(6);
                }
                else
                {
                    InitiatorAddressDisplay = LockTransaction.InitiatorAddress;
                }
            }
        }

        public void Dispose()
        {
            AppBreakpointService.BreakpointChanged -= AppBreakpointService_BreakpointChanged;
        }
    }
}