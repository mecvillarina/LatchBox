using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using Client.Pages.Modals;
using Client.Parameters;
using MudBlazor;
using Neo.SmartContract.Native;
using Neo.Wallets;

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
                    await LockTokenVaultManager.AddLock(walletAccount.KeyPair, PlatformTokenManager.TokenScriptHash, 1000000000, 1, new List<LatchBoxLockReceiverArg> { new LatchBoxLockReceiverArg() { ReceiverAddress = from.ToScriptHash(addressVersion), Amount = 1000000000 } }, true);
                    IsAssetLoaded = true;
                    StateHasChanged();
                });
            }
        }
    }
}