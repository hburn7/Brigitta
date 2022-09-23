using NLog;
using System.Collections.Generic;
using System.Linq;

namespace Brigitta.Models.Irc;

public class TabManager
{
	private readonly IrcWrapper _irc;
	private readonly Logger _logger;

	public TabManager(IrcWrapper irc)
	{
		_logger = LogManager.GetCurrentClassLogger();
		_irc = irc;
		Tabs = new List<ChatTab>();
	}

	public List<ChatTab> Tabs { get; set; }
	public ChatTab CurrentTab { get; private set; }

	public void SwitchToTab(string name)
	{
		_logger.Trace($"Switching to tab {name} (was \"{CurrentTab}\")");

		var existing = GetTab(name);
		if (existing != null)
		{
			CurrentTab = existing;
		}
		else
		{
			AddTab(name);
		}
	}

	public void AddTab(string name)
	{
		Tabs.Add(new ChatTab(_irc, name));
		SwitchToTab(name);
	}

	public void AddTab(ChatTab tab)
	{
		Tabs.Add(tab);
		SwitchToTab(tab.Name);
	}

	public void RemoveTab(string name)
	{
		// todo: set CurrentTab to a valid tab if possible.
		var match = GetTab(name);
		if (match != null)
		{
			Tabs.Remove(match);
		}
	}

	public ChatTab? GetTab(string name) => Tabs.FirstOrDefault(x => x.Name == name);
}