using Client.Infrastructure.Settings;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers.Interfaces
{
    public interface IPreferenceManager
    {
        Task SetPreference(IPreference preference);
        Task<IPreference> GetPreference();
    }
}