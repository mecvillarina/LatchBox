using Client.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Client.Helpers
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

            try
            {
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
            catch
            {
                base.Render(builder);
            }
        }
    }
}
