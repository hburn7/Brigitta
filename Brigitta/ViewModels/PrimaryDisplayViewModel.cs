// ReSharper disable MemberCanBeMadeStatic.Global

using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Brigitta.Models.Irc;
using Brigitta.Views;
using NLog;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CA2011, CA1826

namespace Brigitta.ViewModels;

public class PrimaryDisplayViewModel : ViewModelBase
{
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
	private int _chatFeedFontSize = 8;

	// ReSharper disable once MemberInitializerValueIgnored
	// This is only used to work with the designer
	private List<ChatTab> _chatTabs;
	private string _currentChatDisplay;
	// private List<ChatTab> _mpLobbyTabs;
	private readonly ChatTab _currentTab;
	private ObservableCollection<ChatTab> _selectedTabs = null!;

	public PrimaryDisplayViewModel(IrcWrapper irc)
	{
		Irc = irc;
		TabManager = new TabManager(irc);

		if (Irc.Client.IsConnected)
		{
			TabManager.AddTab(new ChatTab(irc, "BanchoBot", ""));
		}
		else
		{
			TabManager.AddTab(new ChatTab(irc, "BanchoBot", "BanchoBot Test"));
			TabManager.AddTab(new ChatTab(irc, "TheOmyNomy", "TheOmyNomy Test"), false);
			TabManager.AddTab(new ChatTab(irc, "#mp_87654321", "#mp_87654321 Test"), false);
		}

		_chatTabs = TabManager.Tabs;
		// _mpLobbyTabs = new List<ChatTab>();
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

		TabManager.OnTabAdded += _ => { Tabs = TabManager.Tabs; };
		TabManager.OnMessageRoutedToTab += (_, _) => { CurrentChatDisplay = TabManager.CurrentTab.ChatLog; };
		
		Irc.ChatQueue.OnEnqueue += VisuallyDeployMessage;
	}

	// Only used for PrimaryDisplay.axaml -- would never be called in
	// production as the IrcWrapper is always passed from the initial login page.
	public PrimaryDisplayViewModel() : this(new IrcWrapper()) {}
	public TabManager TabManager { get; }
	public ISelectionModel ChatTabSelectionModel { get; }
	public IrcWrapper Irc { get; }
	public List<ChatTab> Tabs
	{
		get => _chatTabs;
		set => this.RaiseAndSetIfChanged(ref _chatTabs, value);
	}
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
	public int MpTimerIncrement => 30;
	public List<string> AutoCompletePhrases => _autoCompletePhrases();

	public void VisuallyDeployMessage(ChatMessage chatMessage)
	{
		if (chatMessage.Sender == null || chatMessage.Content == null)
		{
			return;
		}

		if (chatMessage.IrcCommand.Command == IrcCodes.PrivateMessage && 
		    chatMessage.Sender != Irc.Credentials.Username)
		{
			TabManager.RouteToTab(chatMessage.Sender, chatMessage.Content);
		}
		else
		{
			TabManager.RouteToTab(chatMessage.Recipient, chatMessage.Content);
		}
	}

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
			Irc.Credentials.Username, _currentTab.Name));
	}
}