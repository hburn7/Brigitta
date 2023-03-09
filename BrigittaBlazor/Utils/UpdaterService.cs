using Octokit;

namespace BrigittaBlazor.Utils;

public class UpdaterService
{
	private readonly ILogger<UpdaterService> _logger;
	private readonly GitHubClient _ghClient;

	public const string VERSION = "2.2";

	public UpdaterService(ILogger<UpdaterService> logger, GitHubClient ghClient)
	{
		_logger = logger;
		_ghClient = ghClient;
	}
	
	public async Task<Release?> GetLatestReleaseAsync()
	{
		try
		{
			return await _ghClient.Repository.Release.GetLatest("hburn7", "Brigitta");
		}
		catch (ApiException e)
		{
			_logger.LogWarning("Github encountered an API exception while trying to get the latest release", e);
			return null;
		}
	}

	public async Task<IEnumerable<Release>?> GetRecentReleasesAsync(int amount = 5)
	{
		try
		{
			return (await _ghClient.Repository.Release.GetAll("hburn7", "Brigitta")).Take(amount);
		}
		catch (ApiException e)
		{
			_logger.LogWarning("Github encountered an API exception while trying to get the latest release. " +
			                   "You are probably being ratelimited");
			return null;
		}
	}

	public async Task<bool?> NeedsUpdateAsync()
	{
		_logger.LogInformation("Checking for updates...");
		try
		{
			var latestRelease = await GetLatestReleaseAsync();

			if (latestRelease == null)
			{
				_logger.LogWarning("Could not identify latest release while checking for an update!");
				return null;
			}
			
			string currentVersion = "v" + VERSION;
		
			_logger.LogInformation($"Latest release: {latestRelease.TagName}");
			_logger.LogInformation($"Current version: {currentVersion}");

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
		catch (ApiException e)
		{
			_logger.LogWarning($"Github encountered an API exception while trying to check for updates: {e.Message}");
			return null;
		}
	}
	
	public async Task<IEnumerable<UpdateInfo>> GetRecentUpdateInfosAsync()
	{
		List<UpdateInfo> updates = new();
		try
		{
			var latestReleases = await GetRecentReleasesAsync();
			if (latestReleases == null)
			{
				return updates;
			}
			
			foreach (var release in latestReleases)
			{
				if (!release.Name.StartsWith("v2"))
				{
					continue;
				}

				var commits = release.Body.Split("\n")[1..];
				var updateInfo = new UpdateInfo
				{
					Version = release.Name,
					Url = release.HtmlUrl,
					Commits = commits.Select(c => new Commit
					{
						Description = string.Join(" ", c.Split()[2..]).Trim(),
						Hash = c.Split()[1].Split(':')[0]
					})
				};
				updates.Add(updateInfo);
			}

			return updates;
		}
		catch (ApiException e)
		{
			_logger.LogWarning("Github encountered an API exception while trying to get the latest release", e);
			return updates;
		}
		catch (IndexOutOfRangeException)
		{
			_logger.LogWarning("Changelog contained no valid commits");
			return updates;
		}
	}
}

public class UpdateInfo
{
	public string Version { get; set; }
	public string Url { get; set; }
	public IEnumerable<Commit> Commits { get; set; }
}

public class Commit
{
	public string Description { get; set; }
	public string Hash { get; set; }

	public string Url => $"https://github.com/hburn7/Brigitta/commit/{Hash}";
}