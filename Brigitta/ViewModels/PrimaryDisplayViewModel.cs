// ReSharper disable MemberCanBeMadeStatic.Global

using Avalonia.Controls.Selection;
using BanchoSharp;
using BanchoSharp.Interfaces;
using BanchoSharp.Messaging;
using BanchoSharp.Messaging.ChatMessages;
using Brigitta.Models;
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
	private ObservableCollection<IChatChannel> _chatChannels = null!;
	private int _chatFeedFontSize = 12;
	
	// ReSharper disable once MemberInitializerValueIgnored
	// This is only used to work with the designer
	private string _currentChatDisplay;
	private string _currentChatWatermark = null!;
	private IChatChannel _currentlySelectedChannel;
	private ObservableCollection<IChatChannel> _selectedChannels = null!;

	public PrimaryDisplayViewModel(BanchoClient client)
	{
		Client = client;
		Channels = new ObservableCollection<IChatChannel>(Client.Channels);

		AddTabInteraction = new Interaction<AddTabPromptViewModel, string?>();
		AddTabCommand = ReactiveCommand.CreateFromTask(async () => await HandleTabAddedAsync());

		Client.OnChannelJoined += channel => Channels.Add(channel);
		Client.OnChannelParted += channel => Channels.Remove(channel);
		Client.OnChannelJoinFailure += name =>
		{
			var channel = Channels.FirstOrDefault(x => x.FullName == name);
			if (channel != null)
			{
				Channels.Remove(channel);
			}
		};

		Channels.CollectionChanged += (sender, args) => { _logger.Info("Collection modified."); }; 

		if (Client.IsAuthenticated)
		{
			Client.JoinChannelAsync("BanchoBot").GetAwaiter().GetResult();
		}
		else
		{
			// this is supposed to be for testing the UI. Not sure if it works.
			Channels.Add(new Channel("BanchoBot"));
			Channels.Add(new Channel("TheOmyNomy"));
		}

		ChatTabSelectionModel = new SelectionModel<IChatChannel>();
		ChatTabSelectionModel.Source = Channels;
		ChatTabSelectionModel.SelectionChanged += (_, e) =>
		{
			_logger.Trace("Tab selection changed");
			// Switches to the newly selected tab
			if (e.SelectedItems.Count > 1 || e.DeselectedItems.Count > 1)
			{
				throw new InvalidOperationException("Only one item can be selected and deselected at a time.");
			}

			if (e.SelectedItems.First() is not IChatChannel channel)
			{
				throw new InvalidOperationException("Something other than a chat tab was discovered inside the collection.");
			}

			CurrentlySelectedChannel = channel;
			CurrentChatDisplay = string.Join("\n", channel.MessageHistory!);
		};

		ChatTabSelectionModel.SelectionChanged += (sender, args) =>
		{
			if (args.SelectedItems.Count != 1)
			{
				throw new InvalidOperationException("More than one channel cannot be selected at a time.");
			}

			if (args.SelectedItems.First() is not IChatChannel channel)
			{
				throw new InvalidOperationException($"Selected item was not a channel: {args.SelectedItems.First()}.");
			}

			if (!channel.MessageHistory!.Any())
			{
				if (channel.FullName.StartsWith("#"))
				{
					CurrentChatWatermark = $"Send a message to {channel.FullName}...";
				}
				else
				{
					CurrentChatWatermark = $"Send {channel.FullName} a message...";
				}
			}
			else
			{
				CurrentChatWatermark = "";
			}
		};
	}

	// Only used for PrimaryDisplay.axaml -- would never be called in
	// production as the IrcWrapper is always passed from the initial login page.
	public PrimaryDisplayViewModel() : this(new BanchoClient(new BanchoClientConfig(new CredentialsModel()))) {}
	public ISelectionModel ChatTabSelectionModel { get; }
	public BanchoClient Client { get; }
	public IChatChannel CurrentlySelectedChannel
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
			DataContext = new LobbySetupViewModel(Client, "foo")
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

	public async Task HandleConsoleInputAsync(string text)
	{
		_logger.Trace($"Handling text input: {text}");

		if (text.StartsWith("/"))
		{
			// todo: handle slash commands
			_logger.Debug("Slash command detected");
			return;
		}

		// This is a disgusting solution but oh well
		await Client.SendPrivateMessageAsync(CurrentlySelectedChannel.FullName, text);
		var message = PrivateIrcMessage.CreateFromParameters(Client.ClientConfig.Credentials.Username,
			CurrentlySelectedChannel.FullName, text);

		CurrentlySelectedChannel.MessageHistory!.Push(message);
		CurrentChatDisplay = string.Join("\n", CurrentlySelectedChannel.MessageHistory);
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

		await Client.JoinChannelAsync(tab);
		_logger.Trace($"HandleTabAddedAsync completed successfully ({tab})");
	}

	// Button dispatch
	public async Task DispatchStandardTimer(int seconds) =>
		await Client.SendPrivateMessageAsync(CurrentlySelectedChannel.FullName, $"!mp timer {seconds}");

	public async Task DispatchMatchTimer(int seconds) =>
		await Client.SendPrivateMessageAsync(CurrentlySelectedChannel.FullName, $"!mp start {seconds}");

	public async Task DispatchAbortTimer() => await Client.SendPrivateMessageAsync(CurrentlySelectedChannel.FullName, "!mp aborttimer");
	public async Task DispatchMatchAbort() => await Client.SendPrivateMessageAsync(CurrentlySelectedChannel.FullName, "!mp abort");
}