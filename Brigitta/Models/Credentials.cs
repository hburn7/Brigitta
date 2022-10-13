using BanchoSharp;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;

namespace Brigitta.Models;

public class Credentials : IrcCredentials
{
	private static readonly string SaveRoot = Path.Join(Path.GetTempPath(), "Brigitta");
	private static readonly string CredentialsFileLocation = Path.Join(SaveRoot, "credentials.json");
	private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();
	public string Username { get; init; }
	public string Password { get; init; }
	public bool RememberMe { get; init; }
	public void Save() => Save(this);

	public void Save(Credentials credentials)
	{
		_logger.Trace("Begin save credentials");
		var di = new DirectoryInfo(SaveRoot);
		if (!di.Exists)
		{
			_logger.Trace("Directory not found, creating...");
			di.Create();
			_logger.Trace($"Credentials directory created at {di.FullName}");
		}

		string json = JsonConvert.SerializeObject(credentials, Formatting.Indented);
		File.WriteAllText(CredentialsFileLocation, json);
		_logger.Debug($"Credentials saved to {CredentialsFileLocation}");
	}

	public Credentials Load()
	{
		if (!File.Exists(CredentialsFileLocation))
		{
			Save();
			return Load();
		}

		try
		{
			string json = File.ReadAllText(CredentialsFileLocation);
			var credenditals = JsonConvert.DeserializeObject<Credentials>(json);
			if (credenditals == null)
			{
				_logger.Warn("No stored credentials found.");
				return new Credentials();
			}

			return credenditals;
		}
		catch (Exception e)
		{
			_logger.Error($"Failed to read credentials from file location {e}");
		}

		return new Credentials();
	}

	public Credentials(string username, string password, bool rememberMe) : base(username, password)
	{
		Username = username;
		Password = password;
		RememberMe = rememberMe;
	}
}