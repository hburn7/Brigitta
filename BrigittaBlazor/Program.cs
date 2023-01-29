using BanchoSharp;
using BanchoSharp.Interfaces;
using BrigittaBlazor.Auth;
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

// Add services to the container.
builder.Services.AddAuthenticationCore();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<IBanchoClient, BanchoClient>();
builder.Services.AddScoped<GitHubClient>(_ => new GitHubClient(new ProductHeaderValue("Brigitta")));
builder.Services.AddScoped<UpdaterService>();
// Add serilog as the logging provider with file and console sinks
builder.Services.AddLogging(loggingBuilder =>
{
	loggingBuilder.AddSerilog(dispose: true);
});

Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             .Filter.ByExcluding(Matching.FromSource("Microsoft"))
             .WriteTo.Console()
             .WriteTo.File($"logs/brigitta.log", rollingInterval: RollingInterval.Day)
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
catch (Exception e)
{
	Log.Fatal(e, "Application terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
}
