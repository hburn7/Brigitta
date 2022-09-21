// ReSharper disable MemberCanBeMadeStatic.Global

using Avalonia.Controls;
using Brigitta.Models.Irc;
using Brigitta.Views;
using NLog;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable CA2011

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

	private readonly TabManager _tabManager;

	// ReSharper disable once MemberInitializerValueIgnored
	// This is only used to work with the designer
	private List<ChatTab> _chatTabs;
	private List<ChatTab> _mpLobbyTabs;
	
	private string _currentTab;
	private int _chatFeedFontSize = 8;

	public PrimaryDisplayViewModel(IrcWrapper irc)
	{
		_tabManager = new TabManager();
		_chatTabs = _tabManager.Tabs;
		_mpLobbyTabs = new List<ChatTab>();
		_currentTab = _tabManager.CurrentTab;
		
		Irc = irc;

		AddChatTab(new ChatTab("BanchoBot", "BanchoBot Test"));

		DequeueLoop();
	}

	// Only used for PrimaryDisplay.axaml -- would never be called in
	// production as the IrcWrapper is always passed from the initial login page.
	public PrimaryDisplayViewModel() : this(new IrcWrapper()) {}
	public IrcWrapper Irc { get; }

	public List<ChatTab> Tabs
	{
		get => _chatTabs;
		set => this.RaiseAndSetIfChanged(ref _chatTabs, value);
	}
	
	public List<ChatTab> MpLobbyTabs
	{
		get => _mpLobbyTabs;
		set => this.RaiseAndSetIfChanged(ref _mpLobbyTabs, value);
	}
	
	public void AddChatTab(string? name)
	{
		if (name == null)
		{
			_logger.Warn("Attempted to add null chat tab");
			return;
		}

		AddChatTab(new ChatTab(name));
	}

	public void AddChatTab(ChatTab tab)
	{
		_logger.Trace($"Attempting to add tab: {tab}");

		_tabManager.AddTab(tab);
		Tabs = _tabManager.Tabs;

		if (Irc.Client.IsConnected)
		{
			if (tab.Name.StartsWith("#"))
			{
				Irc.Client.Channels.Join(tab.Name);
			}
			else
			{
				Irc.Client.QueryWho(tab.Name);
			}
		}
	}
	
	public void RemoveChatTab(string name)
	{
		throw new NotImplementedException();
	}

	public void RemoveCurrentTab()
	{
		throw new NotImplementedException();
	}

	private void RouteToTab(string tab, string message)
	{
		var match = _tabManager.GetTab(tab);
		if (match == null)
		{
			AddChatTab(new ChatTab(tab, message));
			_logger.Warn($"Attempted to route message to tab that does not exist. " +
			             $"Tab: {tab} | Content: {message}");

			return;
		}

		match.ChatLog += message;
	}

	public int ChatFeedFontSize
	{
		get => _chatFeedFontSize;
		set => this.RaiseAndSetIfChanged(ref _chatFeedFontSize, value);
	}

	public string CurrentTab
	{
		get => _tabManager.CurrentTab;
		set => this.RaiseAndSetIfChanged(ref _currentTab, value);
	}
	public int MpTimerIncrement => 30;
	
	public List<string> AutoCompletePhrases => _autoCompletePhrases();
	public List<ListBoxItem> TeamRed => _teamGenerator();
	public List<ListBoxItem> TeamBlue => _teamGenerator();

	public void DequeueLoop() => Task.Run(async () =>
	{
		while (true)
		{
			while (Irc.ChatQueue.TryDequeue(out var result))
			{
				if (result.Sender == null)
				{
					return;
				}
				
				string display = $"[{result.IrcCommand}] {result.TimeStampPrint} {result.Content}\n";

				if (result.IrcCommand.Command == IrcCodes.PrivateMessage)
				{
					RouteToTab(result.Sender!, display);
				}
				else
				{
					RouteToTab(result.Recipient, display);
				}

				// if (result.Sender == "BanchoBot")
				// {
				// 	// Parse BanchoBot information
				// 	var parser = new BanchoBotDataParser(result);
				// 	
				// 	// The user is creating a tournament lobby for the first time
				// 	if (parser.IsCreationResponse)
				// 	{
				// 		_logger.Debug("BanchoBot creation response received");
				// 		// todo: add the chat tab
				// 		// todo: link chatboxes to tabs
				// 		// todo: link multiplayer lobby class to appropriate tabs
				// 	}
				// }
			}

			await Task.Delay(250);
		}

		_logger.Fatal("Dequeue loop ceased");
	});
	
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

	private List<ListBoxItem> _teamGenerator()
	{
		// Generates a random name for a player
		var random = new Random();
		const string chars = "qwertyuiopasdfghjklzxcvbnm1234567890-=";

		var ret = new List<ListBoxItem>();
		for (int i = 0; i < 8; i++)
		{
			int size = random.Next(15) + 1;
			string name = "";
			for (int j = 0; j < size; j++)
			{
				name += chars[random.Next(chars.Length)];
			}

			var lbi = new ListBoxItem();
			lbi.Classes.Add("player-listboxitem");
			lbi.Content = name;
			lbi.ContextMenu = new ContextMenu();
			lbi.ContextMenu.Items = new List<IControl>
			{
				new MenuItem { Header = "Move..." },
				new Separator(),
				new MenuItem { Header = "Kick" },
				new MenuItem { Header = "Ban" }
			};

			ret.Add(lbi);
		}

		return ret;
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
		Irc.Client.SendRawMessage($"PRIVMSG {_tabManager.CurrentTab} {text}");
		Irc.ChatQueue.Enqueue(new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage), text, 
			Irc.Credentials.Username, _currentTab));
	}
}