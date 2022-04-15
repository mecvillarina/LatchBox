using Client.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Client.App.Helpers
{
    public class AppRouteView : RouteView
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public AppRouteViewService AppRouteViewService { get; set; }

        protected override void Render(RenderTreeBuilder builder)
        {
            var authorizePage = Attribute.GetCustomAttribute(RouteData.PageType, typeof(AuthorizeAttribute)) != null;
            bool isAuthenticated = AppRouteViewService.IsAuthenticated;

            if (authorizePage && !isAuthenticated)
            {
                NavigationManager.NavigateTo($"/auth/login");
            }
            else if (!authorizePage && isAuthenticated)
            {
                NavigationManager.NavigateTo("/");
            }
            else
            {
                base.Render(builder);
            }
        }
    }
}
