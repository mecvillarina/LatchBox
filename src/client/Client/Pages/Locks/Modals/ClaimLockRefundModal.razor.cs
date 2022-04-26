using Blazored.FluentValidation;
using Client.Infrastructure.Extensions;
using Client.Models;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Neo.Network.RPC;
using Neo.SmartContract.Native;

namespace Client.Pages.Locks.Modals
{
    public partial class ClaimLockRefundModal
    {
        [Parameter] public AssetRefundModel Model { get; set; }
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }

        private async Task SubmitAsync()
        {
            if (Validated)
            {
                IsProcessing = true;

                var account = Utility.GetScriptHash(Model.WalletAddress, ManagerToolkit.NeoProtocolSettings);

                var validateResult = await LockTokenVaultManager.ValidateClaimRefundAsync(account, Model.AssetRefund.TokenScriptHash);

                if (string.IsNullOrEmpty(validateResult.Exception))
                {
                    var gasDetails = $"{((decimal)(validateResult.GasConsumed / Math.Pow(10, NativeContract.GAS.Decimals))).ToAmountDisplay(NativeContract.GAS.Decimals)} {NativeContract.GAS.Symbol}";
                    var fromKey = await AppDialogService.ShowConfirmWalletTransaction(Model.WalletAddress, gasDetails);

                    if (fromKey != null)
                    {
                        try
                        {
                            var result = await LockTokenVaultManager.ClaimRefundAsync(fromKey, Model.AssetRefund.TokenScriptHash);

                            if (result.Executions.First().Notifications.Any(x => x.EventName == "ClaimedRefund"))
                            {
                                AppDialogService.ShowSuccess($"Claim Refund success.");
                                MudDialog.Close();
                            }
                            else
                            {
                                AppDialogService.ShowError($"Claim Refund failed. Reason: {result.Executions.First().ExceptionMessage}");
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

        public void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}