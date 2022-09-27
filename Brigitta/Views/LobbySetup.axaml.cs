using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Brigitta.Views;

public partial class LobbySetup : Window
{
	public LobbySetup()
	{
		InitializeComponent();
#if DEBUG
		this.AttachDevTools();
#endif
	}

	private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
	private void Cancel_OnClick(object? sender, RoutedEventArgs e) => Close();
	private void Submit_OnClick(object? sender, RoutedEventArgs e) => Close();
}