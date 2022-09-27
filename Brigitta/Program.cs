using Avalonia;
using Avalonia.ReactiveUI;
using NLog;
using System;

namespace Brigitta
{
	internal class Program
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		[STAThread]
		public static void Main(string[] args)
		{
			NLogConfig.Init();

			try
			{
				BuildAvaloniaApp()
					.StartWithClassicDesktopLifetime(args);
			}
			catch (Exception e)
			{
				_logger.Fatal($"Application encountered a fatal exception: {e}");
			}
		}

		// Avalonia configuration, don't remove; also used by visual designer.
		public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
		                                                         .UsePlatformDetect()
		                                                         .LogToTrace()
		                                                         .UseReactiveUI();
	}
}