using Client.Infrastructure.Models;
using Client.Pages.Modals;
using MudBlazor;
using Neo;

namespace Client.Pages.Locks
{
    public partial class MyLocksPage
    {
        public bool IsLoaded { get; set; }
        public bool IsAssetLoaded { get; set; }


        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeAsync(async () =>
                {
                    var length = await LockTokenVaultManager.GetLatchBoxLocksLength();
                    var addressVersion = ManagerToolkit.NeoProtocolSettings.AddressVersion;
                    var from = (await WalletManager.GetAddressesAsync()).First();
                    //get wallet Account

                    //try
                    //{
                    //    var s = Neo.Network.RPC.Utility.GetScriptHash("NVh8ZCYi4rUsvBpMZgCb4gbm3bQVCMafWU", ManagerToolkit.NeoProtocolSettings);
                    //}
                    //catch (Exception ex)
                    //{

                    //}
                    //await LockTokenVaultManager.AddLock(walletAccount.KeyPair, PlatformTokenManager.TokenScriptHash, 1000000000, 1, new List<LatchBoxLockReceiverArg> { new LatchBoxLockReceiverArg() { ReceiverAddress = from.ToScriptHash(addressVersion), Amount = 1000000000 } }, true);
                    IsAssetLoaded = true;
                    StateHasChanged();
                });
            }
        }

        public async Task InvokeAddLockModalAsync()
        {
            var assetToken = await InvokeSearchNEP17TokenAsync();

            if (assetToken != null)
            {
                var parameters = new DialogParameters();
                parameters.Add(nameof(AddLockModal.AssetToken), assetToken);

                DialogService.Show<AddLockModal>($"Add New Lock", parameters);
            }
        }

        private async Task<AssetToken> InvokeSearchNEP17TokenAsync()
        {
            var dialog = DialogService.Show<SearchNep17TokenModal>($"NEP-17 Token");
            var dialogResult = await dialog.Result;

            if (!dialogResult.Cancelled)
            {
                var tokenScriptHash = (UInt160)dialogResult.Data;

                return await AssetManager.GetTokenAsync(tokenScriptHash);
            }

            return null;
        }
    }
}