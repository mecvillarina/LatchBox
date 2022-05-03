using Blazored.FluentValidation;
using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Neo.Network.RPC;
using System.Numerics;

namespace Client.Pages.Modals
{
    public partial class BuyPlatformTokenModal
    {
        [Parameter] public AssetToken AssetToken { get; set; }
        [Parameter] public string BuyConversationDisplay { get; set; }
        [Parameter] public BuyPlatformTokenParameter Model { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }
        public List<string> WalletAddresses { get; set; } = new();
        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                WalletAddresses = await WalletManager.GetAddressesAsync();
                Model.WalletAddress = WalletAddresses.First();
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
                    IsProcessing = true;

                    BigInteger actualAmount = Convert.ToDecimal(Model.Amount).ToBigInteger((uint)AssetToken.Decimals);
                 
                    var buyToken = await AssetManager.GetTokenAsync(AssetToken.AssetScriptHash, Model.WalletAddress);

                    if (buyToken.Balance < Convert.ToDecimal(Model.Amount))
                    {
                        AppDialogService.ShowError($"Insufficient {AssetToken.Symbol}.");
                    }
                    else
                    {
                        var fromKey = await AppDialogService.ShowConfirmWalletTransaction(Model.WalletAddress);

                        if (fromKey != null)
                        {
                            try
                            {
                                IsProcessing = true;
                                var buySucceeded = await PlatformTokenManager.BuyPlatformTokenAsync(AssetToken.AssetScriptHash, fromKey, actualAmount);

                                if (buySucceeded)
                                {
                                    AppDialogService.ShowSuccess($"Buy succeeded.");
                                    MudDialog.Close();
                                }
                                else
                                {
                                    AppDialogService.ShowError($"Buy failed.");
                                }
                            }
                            catch (Exception ex)
                            {
                                AppDialogService.ShowError(ex.Message);
                            }
                        }
                    }
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