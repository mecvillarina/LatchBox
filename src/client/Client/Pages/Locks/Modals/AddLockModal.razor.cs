using Blazored.FluentValidation;
using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Infrastructure.Models.Parameters;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Neo.Network.RPC;
using System.Numerics;

namespace Client.Pages.Locks.Modals
{
    public partial class AddLockModal
    {
        [Parameter] public AssetToken AssetToken { get; set; } = new();
        [Parameter] public AddLockParameter Model { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }
        public bool IsLoaded { get; set; }
        public List<string> WalletAddresses { get; set; } = new();
        public DateTime MinDateValue { get; set; }
        public AssetToken PaymentToken { get; set; }
        public string AddLockPaymentFeeDisplay { get; set; }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await FetchFeeAsync();
                Model.TokenScriptHash = AssetToken.AssetScriptHash.ToString();
                WalletAddresses = await WalletManager.GetAddressesAsync();
                Model.WalletAddress = WalletAddresses.First();
                MinDateValue = DateTime.Now;
                Model.UnlockDate = MinDateValue.AddDays(1);
                Model.IsRevocable = true;
                IsLoaded = true;
                StateHasChanged();
            }
        }

        private async Task SubmitAsync()
        {
            if (Validated)
            {
                if (!Model.Receivers.Any())
                {
                    AppDialogService.ShowError($"Please add receivers for your lock.");
                }
                else
                {
                    IsProcessing = true;
                    var token = await AssetManager.GetTokenAsync(AssetToken.AssetScriptHash, Model.WalletAddress);

                    if (token.Balance < Convert.ToDecimal(Model.Receivers.Sum(x => x.Amount)))
                    {
                        AppDialogService.ShowError($"Insufficient {AssetToken.Symbol} balance.");
                    }
                    else
                    {
                        List<LockReceiverParameter> receiversArg = new();
                        BigInteger totalAmount = 0;
                        foreach (var receiver in Model.Receivers)
                        {
                            var address = Utility.GetScriptHash(receiver.ReceiverAddress, ManagerToolkit.NeoProtocolSettings);
                            var actualAmount = Convert.ToDecimal(receiver.Amount).ToBigInteger((uint)AssetToken.Decimals);

                            receiversArg.Add(new LockReceiverParameter()
                            {
                                ReceiverAddress = address,
                                Amount = actualAmount
                            });
                            totalAmount += actualAmount;
                        }

                        var sender = Utility.GetScriptHash(Model.WalletAddress, ManagerToolkit.NeoProtocolSettings);
                        var durationInDays = Model.UnlockDate.Value.Date.Subtract(DateTime.Now.Date).Days;

                        var validateResult = await LockTokenVaultManager.ValidateAddLockAsync(sender, AssetToken.AssetScriptHash, totalAmount, durationInDays, receiversArg, Model.IsRevocable);

                        if (string.IsNullOrEmpty(validateResult.Exception))
                        {
                            var fromKey = await AppDialogService.ShowConfirmWalletTransaction(Model.WalletAddress);

                            if (fromKey != null)
                            {
                                try
                                {
                                    IsProcessing = true;
                                    var addLockResult = await LockTokenVaultManager.AddLockAsync(fromKey, AssetToken.AssetScriptHash, totalAmount, durationInDays, receiversArg, Model.IsRevocable);

                                    if (addLockResult.Executions.First().Notifications.Any(x => x.EventName == "CreatedLatchBoxLock"))
                                    {
                                        AppDialogService.ShowSuccess($"Add Lock success.");
                                        MudDialog.Close();
                                    }
                                    else
                                    {
                                        AppDialogService.ShowError($"Add Lock failed. Reason: {addLockResult.Executions.First().ExceptionMessage}");
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
                    }

                    IsProcessing = false;

                }
            }
        }

        private async Task InvokeAddLockReceiverModalAsync()
        {
            var parameters = new DialogParameters();
            parameters.Add(nameof(AddLockReceiverModal.AssetToken), AssetToken);

            var dialog = DialogService.Show<AddLockReceiverModal>($"Add Lock Receiver", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                var receiver = (AddLockReceiverParameter)dialogResult.Data;

                if (Model.Receivers.Any(x => x.ReceiverAddress == receiver.ReceiverAddress))
                {
                    var currentReceiver = Model.Receivers.First(x => x.ReceiverAddress == receiver.ReceiverAddress);
                    currentReceiver.Amount += receiver.Amount;
                }
                else
                {
                    Model.Receivers.Add(new AddLockReceiverParameter()
                    {
                        Id = Guid.NewGuid(),
                        ReceiverAddress = receiver.ReceiverAddress,
                        Amount = receiver.Amount
                    });
                }
            }
        }

        private void RemoveReceiver(Guid id)
        {
            var receiver = Model.Receivers.FirstOrDefault(x => x.Id == id);
            if (receiver != null)
            {
                Model.Receivers.Remove(receiver);
            }
        }

        private async Task FetchFeeAsync()
        {
            var tokenScriptHash = await LockTokenVaultManager.GetPaymentTokenScriptHashAsync();
            PaymentToken = await AssetManager.GetTokenAsync(tokenScriptHash);
            var addLockPaymentFee = await LockTokenVaultManager.GetPaymentTokenAddLockFeeAsync();
            AddLockPaymentFeeDisplay = $"{addLockPaymentFee.ToAmount(PaymentToken.Decimals).ToAmountDisplay(PaymentToken.Decimals)} {PaymentToken.Symbol}";
        }

        public void Cancel()
        {
            MudDialog.Cancel();
        }

    }
}