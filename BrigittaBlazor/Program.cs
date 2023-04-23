using BanchoSharp;
using BanchoSharp.Interfaces;
using BrigittaBlazor.Auth;
using BrigittaBlazor.Settings;
using BrigittaBlazor.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor;
using MudBlazor.Services;
using Octokit;
using Serilog;
using Serilog.Filters;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.WebHost.UseWebRoot("wwwroot").UseStaticWebAssets();

// Add services to the container.
builder.Services.AddAuthenticationCore();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => {  options.DetailedErrors = true; });
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<IBanchoClient, BanchoClient>();
builder.Services.AddScoped<GitHubClient>(_ => new GitHubClient(new ProductHeaderValue("Brigitta")));
builder.Services.AddScoped<UpdaterService>();
builder.Services.AddScoped<IScrollUtils, ScrollUtils>();

// Required in order to ensure the hotkey listener is initialized only once.
// Without this, if the page is refreshed, the hotkey listener will be initialized again,
// resulting in multiple hotkey listeners.
builder.Services.AddSingleton<EventRegistrationTracker>();
builder.Services.AddSingleton<UserSettings>();

// Add serilog as the logging provider with file and console sinks
builder.Services.AddLogging(loggingBuilder => { loggingBuilder.AddSerilog(dispose: true); });

Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Verbose()
             .Filter.ByExcluding(Matching.FromSource("Microsoft"))
             .WriteTo.Console()
             .WriteTo.File("logs/brigitta.log", rollingInterval: RollingInterval.Day)
             .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddMudServices(config =>
{
	config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;

	config.SnackbarConfiguration.VisibleStateDuration = 2500;
	config.SnackbarConfiguration.HideTransitionDuration = 500;
	config.SnackbarConfiguration.ShowTransitionDuration = 500;
});

builder.Services.AddScoped<AuthenticationStateProvider, BrigittaAuthStateProvider>();

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

try
{
	Log.Information("Launching on http://localhost:5000/ -- Navigate to this address in your browser");
	app.Run();
}
catch (IOException e)
{
	Log.Fatal("It looks like Brigitta is already running or something else is occupying port 5001 on your machine. " +
	          "If Brigitta is already running, you can close it and try again. " +
	          "If something else is occupying port 5001, you can close it and try again. " +
	          "If you are not sure what is occupying port 5001, you can try restarting your PC. " +
	          // ReSharper disable once LogMessageIsSentenceProblem
	          "If you are still having issues, please contact the developer.");
}
catch (Exception e)
{
	Log.Fatal(e, "Application terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
	Console.ReadLine();
}