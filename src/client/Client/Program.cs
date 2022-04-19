using Blazored.LocalStorage;
using Client.Extensions;
using Client.Infrastructure.Managers;
using Client.Infrastructure.Models;
using Client.Services;
using MudBlazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
      .AddHubOptions(options =>
      {
          options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
          options.EnableDetailedErrors = true;
          options.HandshakeTimeout = TimeSpan.FromSeconds(15);
          options.KeepAliveInterval = TimeSpan.FromSeconds(15);
          options.MaximumParallelInvocationsPerClient = 1;
          options.MaximumReceiveMessageSize = 32 * 1024;
          options.StreamBufferCapacity = 10;
      });

#if RELEASE
builder.Services.AddSignalR().AddAzureSignalR();
#endif
builder.Services.Configure<NeoSettings>(builder.Configuration.GetSection("NEO"));
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
builder.Services.AddScoped<IAppDialogService, AppDialogService>();
builder.Services.AddManagers();
builder.Services.AddScoped<AppBreakpointService>();
builder.Services.AddScoped<ClipboardService>();
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