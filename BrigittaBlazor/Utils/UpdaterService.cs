using Octokit;
using System.Reflection;

namespace BrigittaBlazor.Utils;

public class UpdaterService
{
	private readonly ILogger<UpdaterService> _logger;
	private readonly GitHubClient _ghClient;

	public UpdaterService(ILogger<UpdaterService> logger, GitHubClient ghClient)
	{
		_logger = logger;
		_ghClient = ghClient;
	}
	
	private async Task<Release> GetLatestReleaseAsync()
	{
		var releases = await _ghClient.Repository.Release.GetAll("hburn7", "Brigitta");
		return releases[0];
	}

	public async Task<bool> NeedsUpdateAsync()
	{
		_logger.LogInformation("Checking for updates...");
		var latestRelease = await GetLatestReleaseAsync();
		var currentVersion = "v" + Assembly.GetExecutingAssembly().GetName().Version;
		
		_logger.LogInformation($"Latest release: {latestRelease.TagName}");
		_logger.LogInformation($"Current version: {currentVersion}");

		if (currentVersion == "v")
		{
			_logger.LogWarning("Could not resolve version from executing assembly! Please report this to the developer");
			return false;
		}
		
		bool needsUpdate = latestRelease.TagName != currentVersion;

		if (needsUpdate)
		{
			_logger.LogWarning("Update detected! Please install from here: https://github.com/hburn7/Brigitta/releases");
		}
		else
		{
			_logger.LogInformation("You're running the latest version of Brigitta!");
		}

		return needsUpdate;
	}
}