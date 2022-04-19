using Blazored.FluentValidation;
using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Neo.Network.RPC;
using Neo.Wallets;
using System.Numerics;

namespace Client.Pages.Modals
{
    public partial class BuyPlatformTokenModal
    {
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

                if (Model.Amount.CountDecimalPlaces() > Model.CurrencyDecimals)
                {
                    AppDialogService.ShowError($"Amount field decimal places must be less than or equal to {Model.CurrencyDecimals}");
                }
                else
                {
                    IsProcessing = true;

                    var actualAmount = Convert.ToDecimal(Model.Amount).ToBigInteger((uint)Model.CurrencyDecimals);

                    var buyToken = await AssetManager.GetTokenAsync(Model.CurrencyHash, Model.WalletAddress);

                    if (buyToken.Balance < Convert.ToDecimal(Model.Amount))
                    {
                        AppDialogService.ShowError($"Insufficient {Model.Currency}.");
                    }
                    else
                    {
                        var fromKey = await InvokeConfirmWalletTransactionModal(Model.WalletAddress);

                        if (fromKey != null)
                        {
                            try
                            {
                                IsProcessing = true;
                                var buySucceeded = await PlatformTokenManager.BuyPlatformTokenAsync(Model.CurrencyHash, fromKey, actualAmount);

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

        private async Task<KeyPair> InvokeConfirmWalletTransactionModal(string walletAddress)
        {
            var options = new DialogOptions() { MaxWidth = MaxWidth.Small };
            var parameters = new DialogParameters();
            parameters.Add(nameof(ConfirmWalletTransactionModal.Model), new ConfirmWalletTransactionParameter() { WalletAddress = walletAddress });
            var dialog = DialogService.Show<ConfirmWalletTransactionModal>($"Confirmation Wallet Transaction", parameters, options);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                return ((WalletAccountKeyPair)dialogResult.Data).KeyPair;
            }

            return null;
        }

        public void Cancel()
        {
            MudDialog.Cancel();
        }

    }
}