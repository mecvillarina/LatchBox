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
    public partial class VestingPeriodPreviewerModal
    {
        [Parameter] public AssetToken AssetToken { get; set; } = new();
        [Parameter] public VestingPeriod Period { get; set; } = new();
        [Parameter] public List<VestingReceiver> Receivers { get; set; } = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

    }
}