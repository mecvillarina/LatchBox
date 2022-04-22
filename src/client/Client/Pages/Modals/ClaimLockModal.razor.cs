using Blazored.FluentValidation;
using Client.Infrastructure.Models;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Pages.Modals
{
    public partial class ClaimLockModal
    {
        [Parameter] public ClaimLockParameter Model { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }

        private async Task SubmitAsync()
        {
            if (Validated)
            {
                IsProcessing = true;
                var validateResult = await LockTokenVaultManager.ValidateClaimLockAsync(Model.ReceiverHash160, Model.LockIndex);

                if (string.IsNullOrEmpty(validateResult.Exception))
                {
                    var fromKey = await AppDialogService.ShowConfirmWalletTransaction(Model.ReceiverAddress);

                    if (fromKey != null)
                    {
                        try
                        {
                            var addLockResult = await LockTokenVaultManager.ClaimLockAsync(fromKey, Model.LockIndex);

                            if (addLockResult.Executions.First().Notifications.Any(x => x.EventName == "ClaimedLatchBoxLock"))
                            {
                                AppDialogService.ShowSuccess($"Claim Lock success.");
                                MudDialog.Close();
                            }
                            else
                            {
                                AppDialogService.ShowError($"Claim Lock failed. Reason: {addLockResult.Executions.First().ExceptionMessage}");
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