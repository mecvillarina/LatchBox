using Client.Infrastructure.Models;
using Client.Models;
using Client.Pages.Vestings.Modals;
using Client.Parameters;
using MudBlazor;
using Neo;
using System.Numerics;

namespace Client.Pages.Vestings
{
    public partial class MyVestingsPage
    {
        public bool IsLoaded { get; set; }
        public bool IsCompletelyLoaded { get; set; }

        public List<VestingTransactionInitiatorModel> Vestings { get; set; } = new();

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await PageService.EnsureAuthenticatedAsync(async (authenticated) =>
                {
                    if (!authenticated) return;

                    await InvokeAsync(async () =>
                    {
                        await FetchDataAsync();
                    });
                });
            }
        }

        private async Task FetchDataAsync()
        {
            IsLoaded = false;
            IsCompletelyLoaded = false;
            StateHasChanged();

            Vestings.Clear();

            var addresses = await WalletManager.GetAddressesAsync();

            foreach (var address in addresses)
            {
                var transactions = await VestingTokenVaultManager.GetTransactionsByInitiatorAsync(address);

                foreach (var transaction in transactions)
                {
                    Vestings.Add(new VestingTransactionInitiatorModel(transaction));
                }
            }

            Vestings = Vestings.OrderByDescending(x => x.Transaction.CreationTime).ToList();

            IsLoaded = true;
            StateHasChanged();

            foreach (var vesting in Vestings)
            {
                var assetToken = await AssetManager.GetTokenAsync(vesting.Transaction.TokenScriptHash);
                vesting.SetAssetToken(assetToken);
            }

            IsCompletelyLoaded = true;
            StateHasChanged();
        }

        private async Task InvokeAddVestingModalAsync()
        {
            var assetToken = await InvokeSearchNEP17TokenAsync();

            if (assetToken != null)
            {
                var options = new DialogOptions() { MaxWidth = MaxWidth.Medium };
                var parameters = new DialogParameters();
                parameters.Add(nameof(AddVestingModal.AssetToken), assetToken);

                var dialog = DialogService.Show<AddVestingModal>($"Add New Vesting", parameters, options);
                var dialogResult = await dialog.Result;

                if (!dialogResult.Cancelled)
                {
                    await FetchDataAsync();
                }
            }
        }

        private async Task<AssetToken> InvokeSearchNEP17TokenAsync()
        {
            var dialog = DialogService.Show<SearchNep17TokenForVestingModal>($"Search NEP-17 Token");
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                var tokenScriptHash = (UInt160)dialogResult.Data;

                return await AssetManager.GetTokenAsync(tokenScriptHash);
            }

            return null;
        }

        private async Task InvokeRevokeVestingModalAsync(VestingTransactionInitiatorModel vestingModel)
        {
            var vestingIndex = vestingModel.Transaction.VestingIndex;

            var parameters = new DialogParameters();
            parameters.Add(nameof(RevokeVestingModal.VestingTransaction), vestingModel.Transaction);
            parameters.Add(nameof(RevokeVestingModal.Model), new RevokeVestingParameter() { VestingIndex = vestingIndex });

            var dialog = DialogService.Show<RevokeVestingModal>($"Revoke Vesting #{vestingIndex}", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                await FetchDataAsync();
            }
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

        private async Task OnTextToClipboardAsync(string text)
        {
            await ClipboardService.WriteTextAsync(text);
            AppDialogService.ShowSuccess("Contract ScriptHash copied to clipboard.");
        }
    }
}