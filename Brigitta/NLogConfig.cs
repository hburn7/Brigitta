using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;

namespace Brigitta;

public class NLogConfig
{
	private static readonly DirectoryInfo LogDir = new(Path.Join(Directory.GetCurrentDirectory(), "logs", "Brigitta"));
	private static string LogFile => $"{LogDir}/{DateTime.Now:yyyy-MM-dd--HH-mm-ss}.log";

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