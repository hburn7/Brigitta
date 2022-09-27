using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Brigitta.Views;

public partial class AddTabPrompt : Window
{
	public AddTabPrompt()
	{
		InitializeComponent();
#if DEBUG
		this.AttachDevTools();
#endif
	}

	private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
	private void Button_Clicked(object? sender, RoutedEventArgs e) => Close();
}