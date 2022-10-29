using Newtonsoft.Json;
using NLog;
using System;
using System.IO;

namespace Brigitta.Models;

public static class Credentials
{
	private static readonly string SaveRoot = Path.Join(Path.GetTempPath(), "Brigitta");
	private static readonly string CredentialsFileLocation = Path.Join(SaveRoot, "credentials.json");
	private static readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();
	private static readonly JsonSerializerSettings _serializerSettings = new() { TypeNameHandling = TypeNameHandling.All };
	
	public static void Save(CredentialsModel credentials)
	{
		_logger.Trace("Begin save credentials");
		var di = new DirectoryInfo(SaveRoot);
		if (!di.Exists)
		{
			_logger.Trace("Directory not found, creating...");
			di.Create();
			_logger.Trace($"Credentials directory created at {di.FullName}");
		}
		
		string json = JsonConvert.SerializeObject(credentials, Formatting.Indented, _serializerSettings);
		File.WriteAllText(CredentialsFileLocation, json);
		_logger.Debug($"Credentials saved to {CredentialsFileLocation}");
	}

	public static CredentialsModel? Load()
	{
		if (!File.Exists(CredentialsFileLocation))
		{
			Save(new CredentialsModel());
			return Load();
		}

		try
		{
			string json = File.ReadAllText(CredentialsFileLocation);
			var credenditals = JsonConvert.DeserializeObject<CredentialsModel>(json, _serializerSettings);
			if (credenditals == null)
			{
				_logger.Warn("No stored credentials found.");
			}

			return credenditals;
		}
		catch (Exception e)
		{
			_logger.Error($"Failed to read credentials from file location {e}");
		}

		return null;
	}
}