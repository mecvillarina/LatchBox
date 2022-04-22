using Blazored.FluentValidation;
using Client.Infrastructure.Models;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Pages.Modals
{
    public partial class RevokeLockModal
    {
        [Parameter] public LockTransaction LockTransaction { get; set; }
        [Parameter] public RevokeLockParameter Model { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }

        private async Task SubmitAsync()
        {
            if (Validated)
            {
                IsProcessing = true;
                var validateResult = await LockTokenVaultManager.ValidateRevokeLockAsync(LockTransaction.InitiatorHash160, LockTransaction.LockIndex);

                if (string.IsNullOrEmpty(validateResult.Exception))
                {
                    var fromKey = await AppDialogService.ShowConfirmWalletTransaction(LockTransaction.InitiatorAddress);

                    if (fromKey != null)
                    {
                        try
                        {
                            var addLockResult = await LockTokenVaultManager.RevokeLockAsync(fromKey, LockTransaction.LockIndex);

                            if (addLockResult.Executions.First().Notifications.Any(x => x.EventName == "RevokedLatchBoxLock"))
                            {
                                AppDialogService.ShowSuccess($"Revoke Lock success.");
                                MudDialog.Close();
                            }
                            else
                            {
                                AppDialogService.ShowError($"Revoke Lock failed. Reason: {addLockResult.Executions.First().ExceptionMessage}");
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