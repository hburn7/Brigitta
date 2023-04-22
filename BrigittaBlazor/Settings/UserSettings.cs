﻿using Newtonsoft.Json;

namespace BrigittaBlazor.Settings;

public class UserSettings
{
	private readonly ILogger<UserSettings> _logger;
	public UserSettings() {}

	public UserSettings(ILogger<UserSettings> logger)
	{
		_logger = logger;
		var loaded = LoadOrCreate("settings.json");

		KeyBinds = loaded.KeyBinds;

		_logger.LogInformation("Settings loaded");
	}

	public List<UserKeyBind> KeyBinds { get; set; } = new();

	private UserSettings LoadOrCreate(string path)
	{
		_logger.LogDebug("Attempting to load settings");
		if (!File.Exists(path))
		{
			_logger.LogDebug("File settings.json does not exist, creating");
			File.Create(path).Close();
			_logger.LogDebug("Created file settings.json");
		}

		_logger.LogDebug("Loading settings");
		string json = File.ReadAllText(path);
		var settings = JsonConvert.DeserializeObject<UserSettings>(json);
		if (settings == null)
		{
			// Save default settings
			settings = new UserSettings();
			string jsonToSave = JsonConvert.SerializeObject(settings, Formatting.Indented);
			File.WriteAllText(path, jsonToSave);

			_logger.LogDebug("Wrote default settings file to settings.json");
		}

		return settings;
	}

	public void Save()
	{
		string json = JsonConvert.SerializeObject(this, Formatting.Indented);
		File.WriteAllText("settings.json", json);
		_logger.LogInformation("Saved settings");
	}
}