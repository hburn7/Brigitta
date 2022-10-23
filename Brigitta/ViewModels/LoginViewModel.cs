// ReSharper disable MemberCanBeMadeStatic.Global

using Avalonia.Media;
using Avalonia.Threading;
using BanchoSharp;
using BanchoSharp.Interfaces;
using Brigitta.Models;
using Brigitta.Views;
using NLog;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Brigitta.ViewModels;

public class LoginViewModel : ViewModelBase
{
	private CredentialsModel? _credentials;
	private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();
	private BanchoClient _client;
	private string _password;
	private IBrush _passwordBrush;
	private bool _rememberMe;
	private string _username;
	private IBrush _usernameBrush;

	public LoginViewModel()
	{
		_credentials = Credentials.Load();
		_usernameBrush = Palette.FieldDefaultBorderBrush;
		_passwordBrush = Palette.FieldDefaultBorderBrush;

		if (_credentials != null)
		{
			_username = _credentials.Username;
			_password = _credentials.Password;
			_rememberMe = _credentials.RememberMe;
		}
	}

	public string Version => "Version 2022.01.01";
	public string Username
	{
		get => _username;
		set => this.RaiseAndSetIfChanged(ref _username, value);
	}
	public string Password
	{
		get => _password;
		set => this.RaiseAndSetIfChanged(ref _password, value);
	}
	public bool RememberMe
	{
		get => _rememberMe;
		set => this.RaiseAndSetIfChanged(ref _rememberMe, value);
	}

	// todo: move to Palette
	public IBrush UsernameBrush
	{
		get => _usernameBrush;
		set => this.RaiseAndSetIfChanged(ref _usernameBrush, value);
	}
	public IBrush PasswordBrush
	{
		get => _passwordBrush;
		set => this.RaiseAndSetIfChanged(ref _passwordBrush, value);
	}

	public async Task RouteLoginAsync()
	{
		_logger.Trace("RouteLogin init");
		// Route to new UI window -- can be done by opening the new window and closing this one.
		bool invalid = false;
		if (string.IsNullOrWhiteSpace(Username))
		{
			UsernameBrush = Palette.FieldErrorBorderBrush;
			invalid = true;
		}

		if (string.IsNullOrWhiteSpace(Password))
		{
			PasswordBrush = Palette.FieldErrorBorderBrush;
			invalid = true;
		}

		if (invalid)
		{
			_logger.Trace("Invalid, returning");
			return;
		}

		// Set colors to default in case they were red before.
		UsernameBrush = Palette.FieldDefaultBorderBrush;
		PasswordBrush = Palette.FieldDefaultBorderBrush;

		_logger.Trace("Username and password brushes set to default");

		_credentials = new CredentialsModel
		{
			RememberMe = _rememberMe,
			Username = _username,
			Password = _password
		};
		
		if (_rememberMe)
		{
			Credentials.Save(_credentials);
		}
	#if DEBUG
		var logLevel = BanchoSharp.LogLevel.Debug;
	#else
		var logLevel = BanchoSharp.LogLevel.Info;
	#endif
		_client = new BanchoClient(new BanchoClientConfig(_credentials, logLevel));
		_client.OnAuthenticated += async () =>
		{
			await _client.JoinChannelAsync("BanchoBot");
			_logger.Trace("Irc login successful");

			Dispatcher.UIThread.Post(() =>
			{
				var window = new PrimaryDisplay { DataContext = new PrimaryDisplayViewModel(_client) };
				window.Show();
			});
		};

		_client.OnAuthenticationFailed += () =>
		{
			_logger.Warn("Failed to authenticate with osu!Bancho.");
			// Show alert
		};
		
		_logger.Trace("Attempting IRC connection");
		await _client.ConnectAsync();
	}

	public void RouteIrcUrl()
	{
		const string url = "https://osu.ppy.sh/p/irc";
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); // Works ok on windows
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			Process.Start("xdg-open", url); // Works ok on linux
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			Process.Start("open", url); // Not tested
		}
		else
		{
			throw new InvalidOperationException("Platform not supported");
		}
	}
}