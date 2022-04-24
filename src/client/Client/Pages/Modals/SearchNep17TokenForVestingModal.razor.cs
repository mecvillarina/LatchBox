using Blazored.FluentValidation;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Neo.Network.RPC;

namespace Client.Pages.Modals
{
    public partial class SearchNep17TokenForVestingModal
    {
        [Parameter] public SearchNep17TokenParameter Model { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }

        private async Task SubmitAsync()
        {
            if (Validated)
            {
                IsProcessing = true;

                var tokenScriptHash = Utility.GetScriptHash(Model.TokenScriptHash, ManagerToolkit.NeoProtocolSettings);
                var success = await VestingTokenVaultManager.ValidateNEP17TokenAsync(tokenScriptHash);

                if (success)
                {
                    MudDialog.Close(DialogResult.Ok(tokenScriptHash));
                }
                else
                {
                    AppDialogService.ShowError($"Only supports NEP-17 Token and SHOULD have permission for onNEP17Payment or wildcard method.");
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