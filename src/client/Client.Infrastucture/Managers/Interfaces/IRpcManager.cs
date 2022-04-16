using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public interface IRpcManager : IManager
    {
        Task<uint> GetBlock();
    }
}