using Blazored.LocalStorage;
using Client.Extensions;
using Client.Infrastructure.Authentication;
using Client.Infrastructure.Managers;
using Client.Services;
using MudBlazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMudServices(configuration =>
{
    configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
    configuration.SnackbarConfiguration.HideTransitionDuration = 100;
    configuration.SnackbarConfiguration.ShowTransitionDuration = 100;
    configuration.SnackbarConfiguration.VisibleStateDuration = 5000;
    configuration.SnackbarConfiguration.ShowCloseIcon = false;
});
builder.Services.AddScoped<ClientPreferenceManager>();
builder.Services.AddScoped<AppRouteViewService>();
builder.Services.AddScoped<IAppDialogService, AppDialogService>();
builder.Services.AddManagers();
builder.Services.AddScoped<AppBreakpointService>();
//builder.Services.AddScoped<FetchDataExecutor>();
//builder.Services.AddScoped<RenderUIExecutor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

var routeViewService = app.Services.GetRequiredService<AppRouteViewService>();
await routeViewService.PopulateAsync();
