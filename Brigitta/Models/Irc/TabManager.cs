using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Brigitta.Models.Irc;

public class TabManager
{
	public List<ChatTab> Tabs { get; set; }

	public TabManager()
	{
		Tabs = new List<ChatTab>();
	}

	public string CurrentTab { get; private set; } = "";

	public void SwitchToTab(string name) => CurrentTab = name;

	public void AddTab(string name)
	{
		Tabs.Add(new ChatTab(name));
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