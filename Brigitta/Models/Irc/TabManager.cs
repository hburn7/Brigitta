using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Brigitta.Models.Irc;

public class TabManager
{
	private readonly IrcWrapper _irc;
	private readonly Logger _logger;
	private ChatTab _previousTab;

	public TabManager(IrcWrapper irc)
	{
		_logger = LogManager.GetCurrentClassLogger();
		_irc = irc;
		Tabs = new List<ChatTab>();
	}

	public List<ChatTab> Tabs { get; set; }
	public ChatTab CurrentTab { get; private set; }

	/// <summary>
	/// Fired whenever a chat tab is added. Contains the newly added tab.
	/// </summary>
	public Action<ChatTab> OnTabAdded;
	/// <summary>
	/// Fired whenever a message is routed to a chat tab. Contains the tab and the
	/// message that was routed to it.
	/// </summary>
	public Action<ChatTab, string> OnMessageRoutedToTab;
	/// <summary>
	/// Fired whenever a tab is switched. Contains the current tab.
	/// </summary>
	public Action<ChatTab> OnChatTabSwitched;
	/// <summary>
	/// Fired whenever a tab is removed.
	/// </summary>
	public Action<ChatTab> OnChatTabRemoved;

	public void SwitchToTab(string name)
	{
		_logger.Trace($"Switching to tab {name} (was \"{CurrentTab}\")");

		var existing = GetTab(name);
		if (existing != null)
		{
			_previousTab = CurrentTab;

			CurrentTab = existing;
		}
		else
		{
			AddTab(name);
		}
	}
	
	public void RouteToTab(string tab, string message)
	{
		var match = GetTab(tab);
		if (match == null)
		{
			AddTab(new ChatTab(_irc, tab, message));
			_logger.Warn("Attempted to route message to tab that does not exist. " +
			             $"Tab: {tab} | Content: {message}");

			return;
		}

		match.ChatLog += message;
		OnMessageRoutedToTab?.Invoke(match, message);
	}

	public void AddTab(string name, bool swap = true) => AddTab(new ChatTab(_irc, name), swap);

	public void AddTab(ChatTab tab, bool swap = true)
	{
		_logger.Debug($"Tab added: {tab}");
		Tabs.Add(tab);

		if (swap)
		{
			SwitchToTab(tab.Name);
		}

		if (_irc.Client.IsConnected)
		{
			if (tab.Name.StartsWith("#"))
			{
				_irc.Client.Channels.Join(tab.Name);
			}
			else
			{
				_irc.Client.QueryWho(tab.Name);
			}
		}
				
		OnTabAdded?.Invoke(tab);
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