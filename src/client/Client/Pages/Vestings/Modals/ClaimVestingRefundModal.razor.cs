using Blazored.FluentValidation;
using Client.Models;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Neo.Network.RPC;

namespace Client.Pages.Vestings.Modals
{
    public partial class ClaimVestingRefundModal
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

                var validateResult = await VestingTokenVaultManager.ValidateClaimRefundAsync(account, Model.AssetRefund.TokenScriptHash);

                if (string.IsNullOrEmpty(validateResult.Exception))
                {
                    var fromKey = await AppDialogService.ShowConfirmWalletTransaction(Model.WalletAddress);

                    if (fromKey != null)
                    {
                        try
                        {
                            var result = await VestingTokenVaultManager.ClaimRefundAsync(fromKey, Model.AssetRefund.TokenScriptHash);

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