using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Brigitta.ViewModels;
using Brigitta.Views;

namespace Brigitta
{
	public class App : Application
	{
		public override void Initialize() => AvaloniaXamlLoader.Load(this);

		public override void OnFrameworkInitializationCompleted()
		{
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				desktop.MainWindow = new PrimaryDisplay
				{
					DataContext = new PrimaryDisplayViewModel()
				};
				// desktop.MainWindow = new Login
				// {
				// 	DataContext = new LoginViewModel()
				// };
			}

			base.OnFrameworkInitializationCompleted();
		}
	}
}