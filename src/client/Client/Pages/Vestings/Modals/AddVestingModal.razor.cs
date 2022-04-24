using Blazored.FluentValidation;
using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Infrastructure.Models.Parameters;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Neo.Network.RPC;
using System.Numerics;

namespace Client.Pages.Vestings.Modals
{
    public partial class AddVestingModal
    {
        [Parameter] public AssetToken AssetToken { get; set; } = new();
        [Parameter] public AddVestingParameter Model { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }
        public bool IsLoaded { get; set; }
        public List<string> WalletAddresses { get; set; } = new();
        public AssetToken PaymentToken { get; set; }
        public string AddVestingPaymentFeeDisplay { get; set; }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await FetchFeeAsync();
                Model.TokenScriptHash = AssetToken.AssetScriptHash.ToString();
                WalletAddresses = await WalletManager.GetAddressesAsync();
                Model.WalletAddress = WalletAddresses.First();
                Model.IsRevocable = true;
                IsLoaded = true;
                StateHasChanged();
            }
        }

        private async Task SubmitAsync()
        {
            if (Validated)
            {
                if (Model.Periods.Count < 2)
                {
                    AppDialogService.ShowError($"Vesting MUST have at least 2 periods.");
                }
                else if (Model.Periods.Any(x => x.Receivers.Sum(y => y.Amount) == 0))
                {
                    AppDialogService.ShowError($"Each period MUST have at least 1 receiver.");
                }
                else
                {
                    IsProcessing = true;
                    var token = await AssetManager.GetTokenAsync(AssetToken.AssetScriptHash, Model.WalletAddress);

                    if (token.Balance < Convert.ToDecimal(Model.Periods.Sum(x => x.Receivers.Sum(y => y.Amount))))
                    {
                        AppDialogService.ShowError($"Insufficient {AssetToken.Symbol} balance.");
                    }
                    else
                    {
                        List<VestingPeriodParameter> periods = new();
                        BigInteger totalAmount = 0;
                        foreach (var periodModel in Model.Periods)
                        {
                            var period = new VestingPeriodParameter();
                            period.Name = periodModel.Name;
                            period.DurationInDays = periodModel.UnlockDate.Value.Date.Subtract(DateTime.Now.Date).Days;
                            period.Receivers = new();

                            foreach (var receiverModel in periodModel.Receivers)
                            {
                                var address = Utility.GetScriptHash(receiverModel.ReceiverAddress, ManagerToolkit.NeoProtocolSettings);
                                var actualAmount = Convert.ToDecimal(receiverModel.Amount).ToBigInteger((uint)AssetToken.Decimals);

                                period.Receivers.Add(new VestingReceiverParameter()
                                {
                                    Name = receiverModel.Name,
                                    Address = address,
                                    Amount = actualAmount
                                });

                                period.TotalAmount += actualAmount;
                            }

                            periods.Add(period);

                            totalAmount += period.TotalAmount;
                        }

                        var sender = Utility.GetScriptHash(Model.WalletAddress, ManagerToolkit.NeoProtocolSettings);

                        var validateResult = await VestingTokenVaultManager.ValidateAddVestingAsync(sender, AssetToken.AssetScriptHash, totalAmount, Model.IsRevocable, periods);

                        if (string.IsNullOrEmpty(validateResult.Exception))
                        {
                            var fromKey = await AppDialogService.ShowConfirmWalletTransaction(Model.WalletAddress);

                            if (fromKey != null)
                            {
                                try
                                {
                                    IsProcessing = true;
                                    var addVestingResult = await VestingTokenVaultManager.AddVestingAsync(fromKey, AssetToken.AssetScriptHash, totalAmount, Model.IsRevocable, periods);

                                    if (addVestingResult.Executions.First().Notifications.Any(x => x.EventName == "CreatedLatchBoxVesting"))
                                    {
                                        AppDialogService.ShowSuccess($"Add Vesting success.");
                                        MudDialog.Close();
                                    }
                                    else
                                    {
                                        AppDialogService.ShowError($"Add Vesting failed. Reason: {addVestingResult.Executions.First().ExceptionMessage}");
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

        private async Task InvokeUpsetVestingPeriodModalAsync(UpsetVestingPeriodParameter periodModel = null)
        {
            if (!IsProcessing)
            {
                string title = "Add Vesting Period";
                var parameters = new DialogParameters();
                parameters.Add(nameof(UpsetVestingPeriodModal.AssetToken), AssetToken);

                if (periodModel != null)
                {
                    parameters.Add(nameof(UpsetVestingPeriodModal.Model), periodModel);
                    title = "Update Vesting Period";
                }

                var dialog = DialogService.Show<UpsetVestingPeriodModal>(title, parameters);
                var dialogResult = await dialog.Result;

                if (!dialogResult.Cancelled)
                {
                    var period = (UpsetVestingPeriodParameter)dialogResult.Data;

                    if (period.Id == Guid.Empty)
                    {
                        period.Id = Guid.NewGuid();
                        Model.Periods.Add(period);
                    }
                    else
                    {
                        var currentPeriodIndex = Model.Periods.FindIndex(x => x.Id == period.Id);
                        Model.Periods[currentPeriodIndex] = period;
                    }

                    Model.Periods = Model.Periods.OrderBy(x => x.UnlockDate).ToList();
                }
            }
        }


        private void RemovePeriod(Guid id)
        {
            var period = Model.Periods.FirstOrDefault(x => x.Id == id);
            if (period != null)
            {
                Model.Periods.Remove(period);
            }
        }

        private async Task FetchFeeAsync()
        {
            var tokenScriptHash = await VestingTokenVaultManager.GetPaymentTokenScriptHashAsync();
            PaymentToken = await AssetManager.GetTokenAsync(tokenScriptHash);
            var paymentFee = await VestingTokenVaultManager.GetPaymentTokenAddVestingFeeAsync();
            AddVestingPaymentFeeDisplay = $"{paymentFee.ToAmount(PaymentToken.Decimals).ToAmountDisplay(PaymentToken.Decimals)} {PaymentToken.Symbol}";
        }

        public void Cancel()
        {
            MudDialog.Cancel();
        }

    }
}