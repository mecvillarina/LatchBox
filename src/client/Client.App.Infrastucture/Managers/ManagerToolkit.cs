using Blazored.LocalStorage;

namespace Client.App.Infrastructure.Managers
{
    public class ManagerToolkit : IManagerToolkit
    {
        private readonly ILocalStorageService _localStorage;

        public ManagerToolkit(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }
    }
}
