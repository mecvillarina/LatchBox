using Blazored.FluentValidation;
using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Neo.SmartContract.Native;

namespace Client.Pages.Vestings.Modals
{
    public partial class RevokeVestingModal
    {
        [Parameter] public VestingTransaction VestingTransaction { get; set; }
        [Parameter] public RevokeVestingParameter Model { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }
        public bool IsLoaded { get; set; }
        public AssetToken PaymentToken { get; set; }
        public string RevokeVestingPaymentFeeDisplay { get; set; }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await FetchFeeAsync();
                IsLoaded = true;
                StateHasChanged();
            }
        }

        private async Task SubmitAsync()
        {
            if (Validated)
            {
                IsProcessing = true;
                var validateResult = await VestingTokenVaultManager.ValidateRevokeVestingAsync(VestingTransaction.InitiatorHash160, VestingTransaction.VestingIndex);

                if (string.IsNullOrEmpty(validateResult.Exception))
                {
                    var gasDetails = $"{((decimal)(validateResult.GasConsumed / Math.Pow(10, NativeContract.GAS.Decimals))).ToAmountDisplay(NativeContract.GAS.Decimals)} {NativeContract.GAS.Symbol}";
                    var fromKey = await AppDialogService.ShowConfirmWalletTransaction(VestingTransaction.InitiatorAddress, gasDetails);

                    if (fromKey != null)
                    {
                        try
                        {
                            var result = await VestingTokenVaultManager.RevokeVestingAsync(fromKey, VestingTransaction.VestingIndex);

                            if (result.Executions.First().Notifications.Any(x => x.EventName == "RevokedLatchBoxVesting"))
                            {
                                AppDialogService.ShowSuccess($"Revoke Vesting success. Go to My Refunds Page to claim your token refunds.");
                                MudDialog.Close();
                            }
                            else
                            {
                                AppDialogService.ShowError($"Revoke Vesting failed. Reason: {result.Executions.First().ExceptionMessage}");
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

        private async Task FetchFeeAsync()
        {
            var tokenScriptHash = await VestingTokenVaultManager.GetPaymentTokenScriptHashAsync();
            PaymentToken = await AssetManager.GetTokenAsync(tokenScriptHash);
            var paymentFee = await VestingTokenVaultManager.GetPaymentTokenRevokeVestingFeeAsync();
            RevokeVestingPaymentFeeDisplay = $"{paymentFee.ToAmount(PaymentToken.Decimals).ToAmountDisplay(PaymentToken.Decimals)} {PaymentToken.Symbol}";
        }

        public void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}