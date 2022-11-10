// ReSharper disable MemberCanBeMadeStatic.Global

using Avalonia.Controls.Selection;
using BanchoSharp;
using BanchoSharp.Interfaces;
using BanchoSharp.Messaging;
using BanchoSharp.Messaging.ChatMessages;
using Brigitta.Extensions;
using Brigitta.Models;
using Brigitta.Views;
using NLog;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

#pragma warning disable CS8602
#pragma warning disable CA2011, CA1826

namespace Brigitta.ViewModels;

public class PrimaryDisplayViewModel : ViewModelBase
{
	private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();
	private readonly string[] MpCommands =
	{
		"!mp settings", "!mp kick", "!mp ban", "!mp start", "!mp start 5", "!mp start 10", "!mp aborttimer",
		"!mp invite", "!mp lock", "!mp move"
	};
	private readonly string[] SlashCommands =
	{
		"/kick", "/ban", "/clear", "/savelog", "/join", "/quit", "/part"
	};
	private bool _autoScrollEnabled = true;
	private int _caretIndex;
	private ObservableCollection<IChatChannel> _chatChannels = null!;
	private int _chatFeedFontSize = 12;

	// ReSharper disable once MemberInitializerValueIgnored
	// This is only used to work with the designer
	private string _currentChatDisplay;
	private string _currentChatWatermark = null!;
	private IChatChannel? _currentlySelectedChannel;
	private IChatChannel? _previouslySelectedChannel;
	private ObservableCollection<IChatChannel> _selectedChannels = null!;

	public PrimaryDisplayViewModel(BanchoClient client)
	{
		Client = client;
		Channels = new ObservableCollection<IChatChannel>(Client.Channels);

#region TabSelectionModel
		ChatTabSelectionModel = new SelectionModel<IChatChannel>();

		ChatTabSelectionModel.Source = Channels;
		ChatTabSelectionModel.SelectionChanged += (_, e) =>
		{
			_logger.Trace("Tab selection changed");

			object? selection = e.SelectedItems.FirstOrDefault();
			// Switches to the newly selected tab
			if (selection is not IChatChannel channel)
			{
				if (selection != null)
				{
					_logger.Warn($"Something other than a chat tab was discovered inside the collection: {selection}.");
				}

				return;
			}

			if (!channel.MessageHistory!.Any())
			{
				if (channel.ChannelName.StartsWith("#"))
				{
					CurrentChatWatermark = $"Send a message to {channel.ChannelName}...";
				}
				else
				{
					CurrentChatWatermark = $"Send {channel.ChannelName} a message...";
				}
			}
			else
			{
				CurrentChatWatermark = "";
			}

			_previouslySelectedChannel = CurrentlySelectedChannel;
			CurrentlySelectedChannel = channel;
			RefreshChatView();
		};

		if (CurrentlySelectedChannel == null && Channels.Any())
		{
			CurrentlySelectedChannel = Channels.First();
			_previouslySelectedChannel = CurrentlySelectedChannel;

			ChatTabSelectionModel.Select(0);
		}
#endregion

		AddTabInteraction = new Interaction<AddTabPromptViewModel, string?>();
		AddTabCommand = ReactiveCommand.CreateFromTask(async () => await HandleTabAddedAsync());

		Client.OnChannelJoined += channel => { Channels.Add(channel); };
		Client.OnUserQueried += username => { Channels.Add(new Channel(username)); };
		Client.OnChannelParted += channel =>
		{
			Channels.Remove(channel);
			CurrentlySelectedChannel = _previouslySelectedChannel;
		};

		Client.OnChannelJoinFailure += name =>
		{
			var channel = Channels.FirstOrDefault(x => x.ChannelName.Equals(name, StringComparison.OrdinalIgnoreCase));
			if (channel != null)
			{
				Channels.Remove(channel);
			}
		};

		Client.OnPrivateMessageReceived += async message =>
		{
			string routeTo = message.IsDirect ? message.Sender : message.Recipient;
			if (routeTo == "cho.ppy.sh")
			{
				routeTo = "[Server]";
			}

			var channel = Channels.FirstOrDefault(x => x.ChannelName.Equals(routeTo, StringComparison.OrdinalIgnoreCase));
			if (channel == null)
			{
				if (routeTo.StartsWith("#"))
				{
					await Client.JoinChannelAsync(routeTo);
				}
				else
				{
					// DM
					await Client.QueryUserAsync(routeTo);
				}

				channel = Channels.FirstOrDefault(x => x.ChannelName.Equals(routeTo, StringComparison.OrdinalIgnoreCase));
			}

			// Still null? Something went wrong
			if (channel == null)
			{
				_logger.Error($"Failed to route message {message} to chat tab {routeTo}");
				return;
			}

			channel.MessageHistory!.AddLast(message);
			RefreshChatView();
		};

		Channels.CollectionChanged += (sender, args) => { _logger.Info("Collection modified."); };

		if (!Client.IsAuthenticated)
		{
			// this is supposed to be for testing the UI. Not sure if it works.
			Channels.Add(new Channel("BanchoBot"));
			Channels.Add(new Channel("TheOmyNomy"));
			Task.Run(() =>
			{
				for (int i = 0; i < 100; i++)
				{
					Channels.Last()
					        .MessageHistory
					        .AddLast(PrivateIrcMessage.CreateFromParameters("Stage", "TheOmyNomy", i.ToString()));
				}

				RefreshChatView();
			});
		}
	}

	// Only used for PrimaryDisplay.axaml -- would never be called in
	// production as the IrcWrapper is always passed from the initial login page.
	public PrimaryDisplayViewModel() : this(new BanchoClient(new BanchoClientConfig(new CredentialsModel()))) {}
	public ISelectionModel ChatTabSelectionModel { get; }
	public BanchoClient Client { get; }
	public IChatChannel? CurrentlySelectedChannel
	{
		get => _currentlySelectedChannel;
		set => this.RaiseAndSetIfChanged(ref _currentlySelectedChannel, value);
	}
	public ObservableCollection<IChatChannel> Channels
	{
		get => _chatChannels;
		set => this.RaiseAndSetIfChanged(ref _chatChannels, value);
	}
	public Interaction<AddTabPromptViewModel, string?> AddTabInteraction { get; }
	public ICommand AddTabCommand { get; }
	/// <summary>
	///  Should only ever have one element in this collection as only one
	///  chat tab can be selected at a given time
	/// </summary>
	public ObservableCollection<IChatChannel> CurrentlySelectedChannels
	{
		get => _selectedChannels;
		set => this.RaiseAndSetIfChanged(ref _selectedChannels, value);
	}
	public int ChatFeedFontSize
	{
		get => _chatFeedFontSize;
		set => this.RaiseAndSetIfChanged(ref _chatFeedFontSize, value);
	}
	public int ChatFeedCaretIndex
	{
		get => _caretIndex;
		set => this.RaiseAndSetIfChanged(ref _caretIndex, value);
	}
	public bool AutoScrollEnabled
	{
		get => _autoScrollEnabled;
		set => this.RaiseAndSetIfChanged(ref _autoScrollEnabled, value);
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
	public void ToggleAutoScroll() => AutoScrollEnabled = !AutoScrollEnabled;

	public void LobbySetupWindow()
	{
		var window = new LobbySetup
		{
			DataContext = new LobbySetupViewModel(Client, "foo")
		};

		window.Closed += (sender, args) =>
		{
			// Get the data context and use it to populate lobby info.
			_logger.Debug($"Lobby setup window closed. sender={sender} args={args}");
		};

		window.Show();
	}

	public void IncreaseChatFeedFontSize()
	{
		if (ChatFeedFontSize >= 200)
		{
			ChatFeedFontSize = 200;
			return;
		}

		ChatFeedFontSize++;
	}

	public void DecreaseChatFeedFontSize()
	{
		if (ChatFeedFontSize <= 5)
		{
			ChatFeedFontSize = 5;
			return;
		}

		ChatFeedFontSize--;
	}

	public async Task SpawnNewWindow()
	{
		var context = new LoginViewModel();
		context.Username = Client.ClientConfig.Credentials.Username;
		context.Password = Client.ClientConfig.Credentials.Password;
		await context.RouteLoginAsync();
	}

	private List<string> _autoCompletePhrases()
	{
		var ls = new List<string>();
		ls.AddRange(SlashCommands);
		ls.AddRange(MpCommands);
		return ls;
	}

	public async Task HandleConsoleInputAsync(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			_logger.Trace("Discarding empty / whitespace only string from console input.");
			return;
		}

		_logger.Trace($"Handling text input: {text}");

		// Handle slash commands
		if (text.StartsWith("/"))
		{
			var slashCommandHandler = new SlashCommandHandler(text);

			if (slashCommandHandler.RelevantParameters?.Any() ?? false)
			{
				// Switch commands that have expected parameters
				switch (slashCommandHandler.IrcCommand.ToLower())
				{
					case "join":
						await Client.JoinChannelAsync(slashCommandHandler.RelevantParameters[0]);
						break;
					case "part":
						await Client.PartChannelAsync(slashCommandHandler.RelevantParameters[0]);
						break;
					case "away":
						// The only way to be marked as away is to provide a message.
						await Client.SendAsync(string.Join(" ", "AWAY", slashCommandHandler.RelevantParameters[0]));
						DispatchWithoutSendToCurrentTab(text);
						break;
					case "ignore":
						await Client.SendAsync(string.Join(" ", "IGNORE",
							slashCommandHandler.RelevantParameters[0]));
						DispatchWithoutSendToCurrentTab(text);
						break;
					case "unignore":
						await Client.SendAsync(string.Join(" ", "UNIGNORE", slashCommandHandler.RelevantParameters[0]));
						DispatchWithoutSendToCurrentTab(text);
						break;
					case "query":
					case "chat":
					case "msg":
						await Client.QueryUserAsync(slashCommandHandler.RelevantParameters[0]);
						break;
				}
			}
			else
			{
				// Switch commands that don't have expected parameters.
				switch (slashCommandHandler.IrcCommand.ToLower())
				{
					case "away":
						// Marks the user as no longer being away
						await Client.SendAsync("AWAY");
						DispatchWithoutSendToCurrentTab(text);
						break;
					case "clear":
						ClearMessagesFromChannel(_currentlySelectedChannel.ChannelName);
						RefreshChatView();
						break;
					case "quit":
					case "logout":
						await Client.DisconnectAsync();
						
						// Maybe too harsh to quit the program?
						Environment.Exit(0);
						break;
					case "savelog":
						// Save the current chat log
						await SaveMessageLogToFileAsync();
						break;
				}
			}
		}
		else
		{
			await SendAndDispatchToCurrentTabAsync(text);
		}
	}

	public async Task SaveMessageLogToFileAsync()
	{
		// Because we cannot access the SaveFileDialog API from the VM we will force save to
		// currentDirectory/ChatLogs
		
		_logger.Debug("User requested to save message history.");

		if (_currentlySelectedChannel.MessageHistory == null)
		{
			_logger.Warn("Cannot save message history as the BanchoSharp configuration does not allow for this.");
			return;
		}
		
		var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "ChatLogs"));
		if (!di.Exists)
		{
			_logger.Info("Chat history folder not found. Creating...");
			di.Create();
			_logger.Info($"Created chat history folder at {di.FullName}");
		}

		var dt = DateTime.Now;
		string filename = $"{_currentlySelectedChannel.ChannelName}_{dt:s}.txt";
		string savePath = Path.Join(di.FullName, filename);

		var sb = new StringBuilder();
		foreach (var message in _currentlySelectedChannel.MessageHistory.ToList())
		{
			sb.AppendLine(message.ToDisplayString());
		}

		await File.WriteAllTextAsync(savePath, sb.ToString());

		string success = $"Successfully saved message history for {_currentlySelectedChannel.ChannelName} at {savePath}";
		_logger.Info(success);
		DispatchAsBrigittaToCurrentTab(success);
	}

	private void ClearMessagesFromChannel(string name) =>
		Channels.FirstOrDefault(x => x.ChannelName.Equals(name, StringComparison.OrdinalIgnoreCase))?.MessageHistory.Clear();

	/// <summary>
	///  Saves a log of the message in the channel's history but does not
	///  dispatch the message to the IRC server.
	/// </summary>
	/// <param name="content"></param>
	private void DispatchWithoutSendToCurrentTab(string content)
	{
		// Spaghet!
		// What this does is it adds content to the end of the message history of the currently selected channel object.
		// We don't modify currently selected channel as that would not update the correct reference of the channel.
		Channels.FirstOrDefault(x => x.ChannelName
		                              .Equals(_currentlySelectedChannel.ChannelName, StringComparison.OrdinalIgnoreCase))
		        ?.MessageHistory.AddLast(PrivateIrcMessage.CreateFromParameters(Client.ClientConfig.Credentials.Username,
			        _currentlySelectedChannel.ChannelName, content));

		RefreshChatView();
	}

	/// <summary>
	/// Sends a special message to the current chat box as if it was from Brigitta.
	/// </summary>
	/// <param name="content"></param>
	private void DispatchAsBrigittaToCurrentTab(string content)
	{
		Channels.FirstOrDefault(x => x.ChannelName
		                              .Equals(_currentlySelectedChannel.ChannelName, StringComparison.OrdinalIgnoreCase))
		        ?.MessageHistory.AddLast(PrivateIrcMessage.CreateFromParameters("!![BRIGITTA]!!",
			        _currentlySelectedChannel.ChannelName, content));

		RefreshChatView();
	}

	/// <summary>
	///  Sends the message to the specified channel and then adds the message to the UI.
	/// </summary>
	/// <param name="content"></param>
	private async Task SendAndDispatchToCurrentTabAsync(string content)
	{
		string channel = CurrentlySelectedChannel.ChannelName;
		await Client.SendPrivateMessageAsync(channel, content);

		RefreshChatView();
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

		if (tab.StartsWith("#"))
		{
			await Client.JoinChannelAsync(tab);
		}
		else
		{
			await Client.QueryUserAsync(tab);
		}

		_logger.Trace($"HandleTabAddedAsync completed successfully ({tab})");
	}

	private void RefreshChatView()
	{
		if (CurrentlySelectedChannel == null)
		{
			return;
		}

		var sb = new StringBuilder();
		foreach (var msg in CurrentlySelectedChannel.MessageHistory!)
		{
			if (msg is IPrivateIrcMessage privMsg)
			{
				sb.AppendLine(privMsg.ToDisplayString());
			}
			else
			{
				sb.AppendLine(msg.ToDisplayString());
			}
		}

		CurrentChatDisplay = sb.ToString();

		// Update chatbox scroll if necessary
		if (AutoScrollEnabled)
		{
			ChatFeedCaretIndex = CurrentChatDisplay.Length;
		}
	}

	// Button dispatch
	public async Task DispatchStandardTimer(int seconds) => await SendAndDispatchToCurrentTabAsync($"!mp timer {seconds}");
	public async Task DispatchMatchTimer(int seconds) => await SendAndDispatchToCurrentTabAsync($"!mp start {seconds}");
	public async Task DispatchAbortTimer() => await SendAndDispatchToCurrentTabAsync("!mp aborttimer");
	public async Task DispatchMatchAbort() => await SendAndDispatchToCurrentTabAsync("!mp abort");
	public async Task DispatchLock() => await SendAndDispatchToCurrentTabAsync("!mp lock");
	public async Task DispatchUnlock() => await SendAndDispatchToCurrentTabAsync("!mp unlock");
}