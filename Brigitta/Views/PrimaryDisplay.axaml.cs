using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Brigitta.ViewModels;
using NLog;
using ReactiveUI;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Local
namespace Brigitta.Views;

public partial class PrimaryDisplay : ReactiveWindow<PrimaryDisplayViewModel>
{
	private readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public PrimaryDisplay()
	{
#if DEBUG
		this.AttachDevTools();
#endif
		InitializeComponent();
		this.WhenActivated(d => d(ViewModel!.AddTabInteraction.RegisterHandler(DoShowDialogAsync)));
	}

	private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

	private async void TextInput_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key != Key.Return)
		{
			return;
		}

		if (DataContext is not PrimaryDisplayViewModel dataContext)
		{
			return;
		}

		if (sender is not AutoCompleteBox textBox)
		{
			_logger.Warn("Received TextInput_KeyDown event for non AutoCompleteBox");
			return;
		}

		await dataContext.HandleConsoleInputAsync(textBox.Text);
		textBox.Text = "";
		ScrollToBottomOfChatBox();
	}

	public void ScrollToBottomOfChatBox()
	{
		// Force scroll to bottom of text
		var box = this.Get<TextBox>("ChatBox");
		box.CaretIndex = box.Text.Length;
	}

	private async Task DoShowDialogAsync(InteractionContext<AddTabPromptViewModel, string?> interaction)
	{
		var dialog = new AddTabPrompt();
		dialog.DataContext = interaction.Input;

		await dialog.ShowDialog<string>(this);
		interaction.SetOutput(interaction.Input.TabName);
	}
}