﻿using Blazored.FluentValidation;
using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Pages.Modals
{
    public partial class ClaimVestingModal
    {
        [Parameter] public ClaimVestingParameter Model { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }
        public bool IsLoaded { get; set; }
        public AssetToken PaymentToken { get; set; }
        //public string ClaimLockPaymentFeeDisplay { get; set; }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //await FetchFeeAsync();
                IsLoaded = true;
                StateHasChanged();
            }
        }

        private async Task SubmitAsync()
        {
            if (Validated)
            {
                IsProcessing = true;
                var validateResult = await VestingTokenVaultManager.ValidateClaimVestingAsync(Model.ReceiverHash160, Model.VestingIdx, Model.PeriodIdx);

                if (string.IsNullOrEmpty(validateResult.Exception))
                {
                    var fromKey = await AppDialogService.ShowConfirmWalletTransaction(Model.ReceiverAddress);

                    if (fromKey != null)
                    {
                        try
                        {
                            var result = await VestingTokenVaultManager.ClaimVestingAsync(fromKey, Model.VestingIdx, Model.PeriodIdx);

                            if (result.Executions.First().Notifications.Any(x => x.EventName == "ClaimedLatchBoxVesting"))
                            {
                                AppDialogService.ShowSuccess($"Claim Vesting success.");
                                MudDialog.Close();
                            }
                            else
                            {
                                AppDialogService.ShowError($"Claim Vesting failed. Reason: {result.Executions.First().ExceptionMessage}");
                            }
                        }
                        catch (Exception ex)
                        {
                            AppDialogService.ShowError(ex.Message);
                        }
                    }
                }
                else
                {
                    AppDialogService.ShowError(validateResult.Exception);
                }

                IsProcessing = false;
            }
        }

        //private async Task FetchFeeAsync()
        //{
        //    var tokenScriptHash = await LockTokenVaultManager.GetPaymentTokenScriptHashAsync();
        //    PaymentToken = await AssetManager.GetTokenAsync(tokenScriptHash);
        //    var claimLockPaymentFee = await LockTokenVaultManager.GetPaymentTokenClaimLockFee();
        //    ClaimLockPaymentFeeDisplay = $"{claimLockPaymentFee.ToAmount(PaymentToken.Decimals).ToAmountDisplay(PaymentToken.Decimals)} {PaymentToken.Symbol}";
        //}

        public void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}