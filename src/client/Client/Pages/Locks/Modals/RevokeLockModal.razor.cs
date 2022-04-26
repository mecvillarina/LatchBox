using Blazored.FluentValidation;
using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Neo.SmartContract.Native;

namespace Client.Pages.Locks.Modals
{
    public partial class RevokeLockModal
    {
        [Parameter] public LockTransaction LockTransaction { get; set; }
        [Parameter] public RevokeLockParameter Model { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }
        public bool IsLoaded { get; set; }
        public AssetToken PaymentToken { get; set; }
        public string RevokeLockPaymentFeeDisplay { get; set; }

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
                var validateResult = await LockTokenVaultManager.ValidateRevokeLockAsync(LockTransaction.InitiatorHash160, LockTransaction.LockIndex);
                
                if (string.IsNullOrEmpty(validateResult.Exception))
                {
                    var gasDetails = $"{((decimal)(validateResult.GasConsumed / Math.Pow(10, NativeContract.GAS.Decimals))).ToAmountDisplay(NativeContract.GAS.Decimals)} {NativeContract.GAS.Symbol}";
                    var fromKey = await AppDialogService.ShowConfirmWalletTransaction(LockTransaction.InitiatorAddress, gasDetails);

                    if (fromKey != null)
                    {
                        try
                        {
                            var result = await LockTokenVaultManager.RevokeLockAsync(fromKey, LockTransaction.LockIndex);

                            if (result.Executions.First().Notifications.Any(x => x.EventName == "RevokedLatchBoxLock"))
                            {
                                AppDialogService.ShowSuccess($"Revoke Lock success. Go to My Refunds Page to claim your token refunds.");
                                MudDialog.Close();
                            }
                            else
                            {
                                AppDialogService.ShowError($"Revoke Lock failed. Reason: {result.Executions.First().ExceptionMessage}");
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
            var tokenScriptHash = await LockTokenVaultManager.GetPaymentTokenScriptHashAsync();
            PaymentToken = await AssetManager.GetTokenAsync(tokenScriptHash);
            var revokeLockPaymentFee = await LockTokenVaultManager.GetPaymentTokenRevokeLockFee();
            RevokeLockPaymentFeeDisplay = $"{revokeLockPaymentFee.ToAmount(PaymentToken.Decimals).ToAmountDisplay(PaymentToken.Decimals)} {PaymentToken.Symbol}";
        }

        public void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}