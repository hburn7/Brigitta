namespace Brigitta.Models.Irc;

public class ChatTab
{
	public ChatTab(string name)
	{
		Name = name;
		ChatLog = "";
	}

	public ChatTab(string name, string chatLog)
	{
		Name = name;
		ChatLog = chatLog;
	}
	
	public string Name { get; set; }
	public string ChatLog { get; set; }
	public override string ToString() => $"ChatTab(Name={Name}, ChatLog={ChatLog})";
}