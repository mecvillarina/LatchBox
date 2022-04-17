using MudBlazor;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers.Interfaces
{
    public interface IClientPreferenceManager : IPreferenceManager
    {
        MudTheme GetCurrentTheme();
        Task<bool> ToggleDarkModeAsync();
    }
}