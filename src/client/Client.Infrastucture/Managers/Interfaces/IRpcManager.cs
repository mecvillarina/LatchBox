using System.Threading.Tasks;

namespace Client.Infrastructure.Managers.Interfaces
{
    public interface IRpcManager : IManager
    {
        Task<uint> GetBlock();
        string GetRpcUrl();
        string GetNetwork();
    }
}