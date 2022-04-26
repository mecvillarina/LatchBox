using Client.Infrastructure.Managers.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Client.Services
{
    public class PageService
    {
        private readonly NavigationManager _navigationManager;
        private readonly IAuthManager _authManager;
        public PageService(NavigationManager navigationManager, IAuthManager authManager)
        {
            _navigationManager = navigationManager;
            _authManager = authManager;
        }

        public async Task EnsureAuthenticatedAsync(Action<bool> callback)
        {
            var isAuthenticated = await _authManager.IsAuthenticated();

            if (!isAuthenticated)
            {
                _navigationManager.NavigateTo("/");
            }

            callback?.Invoke(isAuthenticated);
        }
    }
}
