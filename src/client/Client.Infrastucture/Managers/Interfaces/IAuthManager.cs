using Microsoft.AspNetCore.Components.Forms;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public interface IAuthManager : IManager
    {
        Task<bool> IsAuthenticated();
        Task Login(IBrowserFile walletFile, string password);
    }
}