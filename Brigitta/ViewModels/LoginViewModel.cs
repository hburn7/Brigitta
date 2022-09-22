﻿// ReSharper disable MemberCanBeMadeStatic.Global

using Avalonia.Media;
using Brigitta.Models;
using Brigitta.Models.Irc;
using Brigitta.Views;
using NLog;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Brigitta.ViewModels
{
	public class LoginViewModel : ViewModelBase
	{
		private static IrcWrapper _irc = new IrcWrapper();
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		private IBrush _usernameBrush;
		private IBrush _passwordBrush;
		
		private string _username = _irc.Credentials.Username;
		private string _password = _irc.Credentials.Password;
		private bool _rememberMe = _irc.Credentials.RememberMe;

		public LoginViewModel()
		{
			_usernameBrush = Palette.FieldDefaultBorderBrush;
			_passwordBrush = Palette.FieldDefaultBorderBrush;
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

		public void RouteLogin()
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

			var updatedCredentials = new Credentials
			{
				Username = _username,
				Password = _password,
				RememberMe = _rememberMe
			};

			_irc = new IrcWrapper(updatedCredentials);
			
			if (_irc.Login())
			{
				_logger.Trace("Irc login successful");
				var window = new PrimaryDisplay { DataContext = new PrimaryDisplayViewModel(_irc) };
				window.Show();
			}
			else
			{
				_logger.Trace("Irc login failed");
				// todo: implement failed login alert of some sort
			}
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
}