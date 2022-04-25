using Client.Infrastructure.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Pages.Vestings.Modals
{
    public partial class VestingPeriodPreviewerModal
    {
        [Parameter] public AssetToken AssetToken { get; set; } = new();
        [Parameter] public VestingTransaction Transaction { get; set; }
        [Parameter] public VestingPeriod Period { get; set; } = new();
        [Parameter] public List<VestingReceiver> Receivers { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

    }
}