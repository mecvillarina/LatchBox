using MudBlazor;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public interface IClientPreferenceManager : IPreferenceManager
    {
        MudTheme GetCurrentTheme();
        Task<bool> ToggleDarkModeAsync();
    }
}