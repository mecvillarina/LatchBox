using Blazored.LocalStorage;
using Client.App.Infrastructure.Managers;
using Client.App.PeriodicExecutors;
using Client.App.Services;
using Client.Infrastructure.Authentication;
using Client.Infrastructure.Managers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Linq;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace Client.App.Extensions
{
    public static class WebAssemblyHostBuilderExtensions
    {
        public static WebAssemblyHostBuilder AddRootComponents(this WebAssemblyHostBuilder builder)
        {
            builder.RootComponents.Add<App>("#app");
            return builder;
        }

        public static WebAssemblyHostBuilder AddClientServices(this WebAssemblyHostBuilder builder)
        {
            builder.Services
                    .AddLocalization(options =>
                     {
                         options.ResourcesPath = "Resources";
                     })
                    .AddAuthorizationCore()
                    .AddBlazoredLocalStorage()
                    .AddMudServices(configuration =>
                    {
                        configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
                        configuration.SnackbarConfiguration.HideTransitionDuration = 100;
                        configuration.SnackbarConfiguration.ShowTransitionDuration = 100;
                        configuration.SnackbarConfiguration.VisibleStateDuration = 5000;
                        configuration.SnackbarConfiguration.ShowCloseIcon = false;
                    })
                    .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())
                    .AddScoped<ClientPreferenceManager>()
                    .AddScoped<AppRouteViewService>()
                    .AddScoped<IAppDialogService, AppDialogService>()
                    //.AddTransient<AppHttpClient>()
                    .AddManagers()
                    .AddScoped<AppBreakpointService>()
                    .AddScoped<FetchDataExecutor>()
                    .AddScoped<RenderUIExecutor>();

#if Release
            builder.Logging.SetMinimumLevel(LogLevel.Critical | LogLevel.Error);
#endif

            return builder;
        }

        public static IServiceCollection AddManagers(this IServiceCollection services)
        {
            var managers = typeof(IManager);

            var types = managers
                    .Assembly
                    .GetExportedTypes()
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .Select(t => new
                    {
                        Service = t.GetInterface($"I{t.Name}"),
                        Implementation = t
                    })
                    .Where(t => t.Service != null);

            foreach (var type in types)
                if (managers.IsAssignableFrom(type.Service))
                    services.AddTransient(type.Service, type.Implementation);

            return services;
        }
    }
}