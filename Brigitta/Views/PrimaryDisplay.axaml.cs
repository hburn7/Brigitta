using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Brigitta.ViewModels;
using NLog;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Local

namespace Brigitta.Views;

public partial class PrimaryDisplay : Window
{
	private readonly Logger _logger = LogManager.GetCurrentClassLogger();
	
	public PrimaryDisplay()
	{
#if DEBUG
		this.AttachDevTools();
#endif
		
		InitializeComponent();
	}

	private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

	private void TextInput_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key != Key.Return)
		{
			return;
		}

		if (this.DataContext is not PrimaryDisplayViewModel dataContext)
		{
			return;
		}

		if (sender is not AutoCompleteBox textBox)
		{
			_logger.Warn("Received TextInput_KeyDown event for non AutoCompleteBox");
			return;
		}
		
		dataContext.HandleConsoleInput(textBox.Text);
		textBox.Text = "";
	}

	private async void AddTab_Clicked(object? sender, RoutedEventArgs e)
	{
		var prompt = new AddTabPrompt
		{
			DataContext = new AddTabPromptViewModel()
		};

		var task = prompt.ShowDialog(this);
		await task.WaitAsync(CancellationToken.None);
		
		PrimaryDisplayViewModel vm = (PrimaryDisplayViewModel)this.DataContext!;

		var context = (AddTabPromptViewModel)prompt.DataContext;

		if (context.TabName != null)
		{
			vm.AddChatTab(context.TabName);
		}
	}
}