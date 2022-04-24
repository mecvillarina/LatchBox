using Blazored.FluentValidation;
using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Pages.Vestings.Modals
{
    public partial class AddVestingReceiverModal
    {
        [Parameter] public AssetToken AssetToken { get; set; } = new();
        [Parameter] public AddVestingReceiverParameter Model { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }
        public List<string> WalletAddresses { get; set; } = new();

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                StateHasChanged();
            }
        }

        private async Task SubmitAsync()
        {
            if (Validated)
            {
                if (Model.Amount.CountDecimalPlaces() > AssetToken.Decimals)
                {
                    if (AssetToken.Decimals > 0)
                    {
                        AppDialogService.ShowError($"Allowable decimal places for 'Amount' less than or equal to {AssetToken.Decimals}");
                    }
                    else
                    {
                        AppDialogService.ShowError($"{AssetToken.Name} doesn't allow 'Amount' to have decimal places.");
                    }
                }
                else
                {
                    MudDialog.Close(DialogResult.Ok(Model));
                }
            }
        }

        public void Cancel()
        {
            MudDialog.Cancel();
        }

    }
}