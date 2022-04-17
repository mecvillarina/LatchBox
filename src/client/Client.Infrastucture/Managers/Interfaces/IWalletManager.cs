using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public interface IWalletManager : IManager
    {
        Task<List<string>> GetAddressesAsync();
    }
}