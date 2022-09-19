using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Brigitta.Views
{
	public partial class Login : Window
	{
		public Login()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
		}
		
		private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
	}
}