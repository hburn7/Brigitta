using IrcDotNet;
using System;

namespace Brigitta.Models.Irc;

public class ChatTab
{
	private readonly IrcWrapper _irc;

	public ChatTab(IrcWrapper irc, string name) : this(irc, name, "")
	{
		Name = name;
	}

	public ChatTab(IrcWrapper irc, string name, string chatLog)
	{
		_irc = irc;
		Name = name;
		ChatLog = chatLog;
		IsMpLobby = name.StartsWith("#");

		// if (IsMpLobby)
		// {
		// 	MpLobby = GetMpLobbyData();
		// }
	}
	
	public string Name { get; set; }
	public string ChatLog { get; set; }
	public bool IsMpLobby { get; set; }
	public MultiplayerLobby? MpLobby { get; set; }
	
	public override string ToString() => Name;

	private MultiplayerLobby GetMpLobbyData()
	{
		// Executes !mp settings and parses the result
		throw new NotImplementedException();
	}

}