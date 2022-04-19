using Microsoft.AspNetCore.Components.Forms;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers.Interfaces
{
    public interface IAuthManager : IManager
    {
        Task<bool> IsAuthenticated();
        Task ConnectWallet(IBrowserFile walletFile, string password);
        Task DisconnectWalletAsync();
    }
}