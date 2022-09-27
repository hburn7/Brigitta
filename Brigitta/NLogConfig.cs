using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;

namespace Brigitta;

public class NLogConfig
{
	private static readonly DirectoryInfo LogDir = new($"{CrawlUp()}/Logs");
	private static string LogFile => $"{LogDir}/{DateTime.Now:yyyy-MM-dd--HH-mm-ss}.txt";

	public static void Init()
	{
		bool canLogToFile = EnsureValidLogPath(LogDir);

		var config = new LoggingConfiguration();
		var logFile = new FileTarget("logfile") { FileName = LogFile };
		var logConsole = new ConsoleTarget("logconsole");

		// Rules for mapping loggers to targets   
#if DEBUG
		config.AddRule(LogLevel.Trace, LogLevel.Fatal, logConsole);
#else
		config.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);
#endif

		if (canLogToFile)
		{
			config.AddRule(LogLevel.Trace, LogLevel.Fatal, logFile);
		}

		// Apply config           
		LogManager.Configuration = config;
	}

	/// <summary>
	///  Crawls up the tree until the desired path is found.
	/// </summary>
	/// <returns></returns>
	private static DirectoryInfo CrawlUp()
	{
		// Look for the directory containing the .csproj file
		var curDir = new DirectoryInfo(Directory.GetCurrentDirectory());
		while (curDir.Parent != null)
		{
			foreach (var file in curDir.EnumerateFiles())
			{
				if (file.Name == "Brigitta.csproj")
				{
					return curDir;
				}
			}

			curDir = curDir.Parent;
		}

		// todo: fallback to appdata or something?
		throw new InvalidOperationException("csproj not found");
	}

	private static bool EnsureValidLogPath(DirectoryInfo dir)
	{
		if (!dir.Exists)
		{
			try
			{
				dir.Create();
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine($"Critical: Cannot log to file -- {e}");
				return false;
			}
		}

		return true;
	}
}