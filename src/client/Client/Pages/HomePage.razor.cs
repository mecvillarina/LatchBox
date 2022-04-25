using Client.Pages.Locks.Modals;
using Client.Pages.Vestings.Modals;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Numerics;

namespace Client.Pages
{
    public partial class HomePage
    {
        [Parameter]
        public long? LockIndex { get; set; }

        [Parameter]
        public long? VestingIndex { get; set; }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (LockIndex.HasValue && LockIndex.Value >= 0)
                {
                    InvokeLockPreviewerModal(LockIndex.Value);
                }
                else if (VestingIndex.HasValue && VestingIndex.Value >= 0)
                {
                    InvokeVestingPreviewerModal(VestingIndex.Value);
                }
            }
        }

        private void InvokeLockPreviewerModal(BigInteger lockIndex)
        {
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium };
            var parameters = new DialogParameters()
            {
                 { nameof(LockPreviewerModal.LockIndex), lockIndex},
            };

            DialogService.Show<LockPreviewerModal>($"Lock #{lockIndex}", parameters, options);
        }

        private void InvokeVestingPreviewerModal(BigInteger vestingIndex)
        {
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium };
            var parameters = new DialogParameters()
            {
                 { nameof(VestingPreviewerModal.VestingIndex), vestingIndex},
            };

            DialogService.Show<VestingPreviewerModal>($"Vesting #{vestingIndex}", parameters, options);
        }
    }
}