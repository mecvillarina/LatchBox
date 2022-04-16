using Microsoft.AspNetCore.Components.Forms;
using Neo.Wallets;
using Neo.Wallets.NEP6;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public class AuthManager : ManagerBase, IAuthManager
    {
        public AuthManager(IManagerToolkit managerToolkit) : base(managerToolkit)
        {
        }

        public async Task<bool> Login(IBrowserFile walletFile, string password)
        {
            string filename = $"{Guid.NewGuid()}.json";
            var tempFilePath = $"{ManagerToolkit.FilePathTemp}/{filename}";

            using (MemoryStream ms = new MemoryStream())
            {
                await walletFile.OpenReadStream().CopyToAsync(ms);
                File.WriteAllBytes(tempFilePath, ms.ToArray());
            }

            NEP6Wallet wallet = new NEP6Wallet(tempFilePath, ManagerToolkit.NeoProtocolSettings);
            try
            {
                KeyPair keyPair2;
                using (wallet.Unlock(password))
                {
                    var addressess = wallet.GetAccounts().Select(x => x.Address).ToList();
                    var walletFilePath = $"{ManagerToolkit.FilePathWallet}/{filename}";
                    File.Copy(tempFilePath, walletFilePath);
                    DeleteFile(tempFilePath);
                    await ManagerToolkit.SaveWalletAsync(filename, addressess);
                    return true;
                }
            }
            catch
            {

            }

            return false;
        }

        private void DeleteFile(string tempFilePath)
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
}
