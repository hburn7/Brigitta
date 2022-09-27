// ReSharper disable MemberCanBeMadeStatic.Global

using Avalonia.Controls.Selection;
using Brigitta.Extensions;
using Brigitta.Models.Irc;
using Brigitta.Views;
using NLog;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#pragma warning disable CA2011, CA1826

namespace Brigitta.ViewModels;

public class PrimaryDisplayViewModel : ViewModelBase
{
	private readonly ChatTab _currentTab;
	private readonly Logger _logger = LogManager.GetCurrentClassLogger();
	private readonly string[] MpCommands =
	{
		"!mp settings", "!mp kick", "!mp ban", "!mp start", "!mp start 5", "!mp start 10", "!mp aborttimer",
		"!mp invite", "!mp lock", "!mp move"
	};
	private readonly string[] SlashCommands =
	{
		"/kick", "/ban", "/clear", "/savelog", "/join", "/quit", "/part"
	};
	private int _chatFeedFontSize = 12;
	private ObservableCollection<ChatTab> _chatTabs = new();

	// ReSharper disable once MemberInitializerValueIgnored
	// This is only used to work with the designer
	private string _currentChatDisplay;
	private string _currentChatWatermark = null!;
	private ObservableCollection<ChatTab> _selectedTabs = null!;

	public PrimaryDisplayViewModel(IrcWrapper irc)
	{
		Irc = irc;
		TabManager = new TabManager(irc);
		Tabs = TabManager.Tabs;

		AddTabInteraction = new Interaction<AddTabPromptViewModel, string?>();
		AddTabCommand = ReactiveCommand.CreateFromTask(async () => await HandleTabAddedAsync());

		if (Irc.Client.IsConnected)
		{
			TabManager.AddTab(new ChatTab(irc, "BanchoBot", ""));
		}
		else
		{
			TabManager.AddTab(new ChatTab(irc, "BanchoBot", "04:31:55 Stage: !help\n04:31:57 BanchoBot: Standard Commands (!COMMAND or /msg BanchoBot COMMAND):\n04:31:57 BanchoBot: WHERE <user>\n04:31:57 BanchoBot: STATS <user>\n04:31:57 BanchoBot: FAQ <item>|list\n04:31:57 BanchoBot: REPORT <reason> - call for an admin\n04:31:57 BanchoBot: REQUEST [list] - shows a random recent mod request\n04:31:57 BanchoBot: ROLL <number> - roll a dice and get random result from 1 to number(default 100)\n04:31:58 Stage: \n04:32:03 Stage: !roll 50\n04:32:15 Stage: !roll 100\n04:32:19 Stage: !roll\n"));
			TabManager.AddTab(new ChatTab(irc, "TheOmyNomy", "TheOmyNomy Test"), false);
			TabManager.AddTab(new ChatTab(irc, "#mp_87654321", "#mp_87654321 Test"), false);
			TabManager.AddTab(new ChatTab(irc, "test", "test Test"), false);
		}

		_currentTab = TabManager.CurrentTab;
		_currentChatDisplay = _currentTab.ChatLog;

		ChatTabSelectionModel = new SelectionModel<ChatTab>();
		ChatTabSelectionModel.Source = Tabs;
		ChatTabSelectionModel.SelectionChanged += (_, e) =>
		{
			_logger.Trace("Tab selection changed");
			// Switches to the newly selected tab
			if (e.SelectedItems.Count > 1 || e.DeselectedItems.Count > 1)
			{
				throw new InvalidOperationException("Only one item can be selected and deselected at a time.");
			}

			if (e.SelectedItems.First() is not ChatTab tab)
			{
				throw new InvalidOperationException("Something other than a chat tab was discovered inside the collection.");
			}

			TabManager.SwitchToTab(tab.Name);
			CurrentChatDisplay = TabManager.CurrentTab.ChatLog;
		};

		TabManager.OnMessageRoutedToTab += (_, _) => { CurrentChatDisplay = TabManager.CurrentTab.ChatLog; };
		TabManager.OnChatTabSwitched += tab =>
		{
			if (string.IsNullOrWhiteSpace(tab.ChatLog))
			{
				if (tab.IsPublicChannel || tab.IsMpLobby)
				{
					CurrentChatWatermark = $"Send a message to {tab}...";
				}
				else
				{
					CurrentChatWatermark = $"Send {tab} a message...";
				}
			}
			else
			{
				CurrentChatWatermark = "";
			}
		};

		Irc.ChatQueue.OnEnqueue += TabManager.RouteToTab;
	}

	// Only used for PrimaryDisplay.axaml -- would never be called in
	// production as the IrcWrapper is always passed from the initial login page.
	public PrimaryDisplayViewModel() : this(new IrcWrapper()) {}
	public TabManager TabManager { get; }
	public ISelectionModel ChatTabSelectionModel { get; }
	public IrcWrapper Irc { get; }
	public ObservableCollection<ChatTab> Tabs
	{
		get => _chatTabs;
		set => this.RaiseAndSetIfChanged(ref _chatTabs, value);
	}
	public Interaction<AddTabPromptViewModel, string?> AddTabInteraction { get; }
	public ICommand AddTabCommand { get; }
	/// <summary>
	///  Should only ever have one element in this collection as only one
	///  chat tab can be selected at a given time
	/// </summary>
	public ObservableCollection<ChatTab> CurrentlySelectedTabs
	{
		get => _selectedTabs;
		set => this.RaiseAndSetIfChanged(ref _selectedTabs, value);
	}
	public int ChatFeedFontSize
	{
		get => _chatFeedFontSize;
		set => this.RaiseAndSetIfChanged(ref _chatFeedFontSize, value);
	}
	public string CurrentChatDisplay
	{
		get => _currentChatDisplay;
		set => this.RaiseAndSetIfChanged(ref _currentChatDisplay, value);
	}
	public string CurrentChatWatermark
	{
		get => _currentChatWatermark;
		set => this.RaiseAndSetIfChanged(ref _currentChatWatermark, value);
	}
	public int MpTimerIncrement => 30;
	public List<string> AutoCompletePhrases => _autoCompletePhrases();

	public void LobbySetupWindow()
	{
		var window = new LobbySetup
		{
			DataContext = new LobbySetupViewModel(Irc, "foo")
		};

		window.Closed += (sender, args) =>
		{
			// Get the data context and use it to populate lobby info.
			_logger.Debug($"Lobby setup window closed. sender={sender} args={args}");
		};

		window.Show();
	}

	public void IncreaseChatFeedFontSize() => ChatFeedFontSize++;
	public void DecreaseChatFeedFontSize() => ChatFeedFontSize--;

	private List<string> _autoCompletePhrases()
	{
		var ls = new List<string>();
		ls.AddRange(SlashCommands);
		ls.AddRange(MpCommands);
		return ls;
	}

	public void HandleConsoleInput(string text)
	{
		_logger.Debug($"Handling text input: {text}");

		if (text.StartsWith("/"))
		{
			// todo: handle slash commands
			_logger.Debug("Slash command detected");
			return;
		}

		// This is a disgusting solution but oh well
		Irc.Client.SendRawMessage($"PRIVMSG {TabManager.CurrentTab} {text}");
		Irc.ChatQueue.Enqueue(new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage), text,
			Irc.Credentials.Username, TabManager.CurrentTab.Name));
	}

	/// <summary>
	///  Handles the visual add tab prompt
	/// </summary>
	public async Task HandleTabAddedAsync()
	{
		string? tab = await AddTabInteraction.Handle(new AddTabPromptViewModel());
		if (string.IsNullOrWhiteSpace(tab))
		{
			_logger.Debug("Attempt to add tab with null or whitespace name occurred. (Action aborted)");
			return;
		}

		TabManager.AddTab(tab);
		_logger.Trace($"HandleTabAddedAsync completed successfully ({tab})");
	}

	// Button dispatch
	public void DispatchStandardTimer(int seconds) => Irc.SendMessage(TabManager.CurrentTab.Name, $"!mp timer {seconds}");
	public void DispatchMatchTimer(int seconds) => Irc.SendMessage(TabManager.CurrentTab.Name, $"!mp start {seconds}");

	public void DispatchAbortTimer() => Irc.SendMessage(TabManager.CurrentTab.Name, "!mp aborttimer");
	public void DispatchMatchAbort() => Irc.SendMessage(TabManager.CurrentTab.Name, "!mp abort");
}