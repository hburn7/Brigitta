using NLog;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Brigitta.Models.Irc;

public class TabManager
{
	private readonly IrcWrapper _irc;
	private readonly Logger _logger;
	private ChatTab _previousTab;
	/// <summary>
	///  Fired whenever a tab is removed.
	/// </summary>
	public Action<ChatTab> OnChatTabRemoved;
	/// <summary>
	///  Fired whenever a tab is switched. Contains the current tab.
	/// </summary>
	public Action<ChatTab> OnChatTabSwitched;
	/// <summary>
	///  Fired whenever a message is routed to a chat tab. Contains the tab and the
	///  message that was routed to it.
	/// </summary>
	public Action<ChatTab, string> OnMessageRoutedToTab;
	/// <summary>
	///  Fired whenever a chat tab is added. Contains the newly added tab.
	/// </summary>
	public Action<ChatTab> OnTabAdded;

	public TabManager(IrcWrapper irc)
	{
		_logger = LogManager.GetCurrentClassLogger();
		_irc = irc;
		Tabs = new ObservableCollection<ChatTab>();
	}

	public ObservableCollection<ChatTab> Tabs { get; set; }
	public ChatTab CurrentTab { get; private set; }

	public void SwitchToTab(string name)
	{
		var existing = GetTab(name);
		if (existing != null)
		{
			SwitchToTab(existing);
		}
		else
		{
			AddTab(name);
		}
	}

	public void SwitchToTab(ChatTab tab)
	{
		_logger.Trace($"Switching to tab {tab.Name} (was \"{CurrentTab}\")");

		_previousTab = CurrentTab;
		CurrentTab = tab;

		OnChatTabSwitched?.Invoke(tab);
	}

	public void RouteToTab(ChatMessage chatMessage)
	{
		if (chatMessage.Sender == null || chatMessage.Content == null)
		{
			return;
		}

		string? target;
		if (chatMessage.IrcCommand.Command == IrcCodes.PrivateMessage &&
		    chatMessage.Sender != _irc.Credentials.Username)
		{
			target = chatMessage.Sender;
		}
		else
		{
			target = chatMessage.Recipient;
		}

		if (target == null)
		{
			return;
		}

		var match = GetTab(target);
		if (match == null)
		{
			AddTab(new ChatTab(_irc, target, chatMessage.Content));
			_logger.Warn("Attempted to route message to tab that does not exist. " +
			             $"Tab: {match} | Content: {chatMessage.Content}");

			return;
		}

		string log = chatMessage.ConsoleLogLine();

		match.ChatLog += log;
		OnMessageRoutedToTab?.Invoke(match, log);
	}

	public void AddTab(string name, bool swap = true) => AddTab(new ChatTab(_irc, name), swap);

	public void AddTab(ChatTab tab, bool swap = true)
	{
		_logger.Debug($"Tab added: {tab}");
		Tabs.Add(tab);
		OnTabAdded?.Invoke(tab);

		if (swap)
		{
			SwitchToTab(tab);
		}

		if (_irc.Client.IsConnected)
		{
			if (tab.Name.StartsWith("#"))
			{
				_logger.Trace($"Joining channel {tab.Name}");
				_irc.Client.Channels.Join(tab.Name);
			}
			else
			{
				_logger.Trace($"Querying user {tab.Name}");
				_irc.Client.QueryWho(tab.Name);
			}
		}
	}

	public void RemoveTab(string name)
	{
		// todo: set CurrentTab to a valid tab if possible.
		var match = GetTab(name);

		if (match == null)
		{
			return;
		}

		Tabs.Remove(match);
		OnChatTabRemoved?.Invoke(match);
	}

	public ChatTab? GetTab(string name) => Tabs.FirstOrDefault(x => x.Name == name);
}