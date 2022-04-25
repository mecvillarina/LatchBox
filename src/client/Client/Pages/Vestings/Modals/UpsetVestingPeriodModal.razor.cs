using Blazored.FluentValidation;
using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Parameters;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Neo.Network.RPC;
using System.Numerics;

namespace Client.Pages.Vestings.Modals
{
    public partial class UpsetVestingPeriodModal
    {
        [Parameter] public AssetToken AssetToken { get; set; } = new();
        [Parameter] public UpsetVestingPeriodParameter Model { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private FluentValidationValidator _fluentValidationValidator;
        private bool Validated => _fluentValidationValidator.Validate(options => { options.IncludeAllRuleSets(); });
        public bool IsProcessing { get; set; }
        public bool IsLoaded { get; set; }
        public DateTime MinDateValue { get; set; }
        public bool IsAdd { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                MinDateValue = DateTime.Now;

                if (!Model.UnlockDate.HasValue)
                {
                    IsAdd = true;
                    Model.UnlockDate = MinDateValue.AddDays(1);
                }

                IsLoaded = true;
                StateHasChanged();
            }
        }

        private void Submit()
        {
            if (Validated)
            {
                if (!Model.Receivers.Any())
                {
                    AppDialogService.ShowError($"Period MUST have at least 1 receiver.");
                }
                else
                {
                    MudDialog.Close(DialogResult.Ok(Model));
                }
            }
        }

        private async Task InvokeAddVestingReceiverModalAsync()
        {
            var parameters = new DialogParameters();
            parameters.Add(nameof(AddVestingReceiverModal.AssetToken), AssetToken);

            var dialog = DialogService.Show<AddVestingReceiverModal>($"Add Vesting Receiver", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                var receiver = (AddVestingReceiverParameter)dialogResult.Data;

                if (Model.Receivers.Any(x => x.ReceiverAddress == receiver.ReceiverAddress))
                {
                    var currentReceiver = Model.Receivers.First(x => x.ReceiverAddress == receiver.ReceiverAddress);
                    currentReceiver.Amount += receiver.Amount;
                }
                else
                {
                    receiver.Id = Guid.NewGuid();
                    Model.Receivers.Add(receiver);
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

        public void Cancel()
        {
            MudDialog.Cancel();
        }

    }
}