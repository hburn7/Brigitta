using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace Brigitta.Models.Irc;

public enum TeamAssignment
{
	Red,
	Blue,
	None
}

// Maybe add !mp addref tracking as well
public class MultiplayerLobby
{
	public MultiplayerLobby(string name, Uri history, LobbyFormat format, TextBox chatLog)
	{
		Name = name;
		History = history;
		Format = format;
		ChatLog = chatLog;
	}
	
	public string Name { get; init; }
	public Uri History { get; init; }
	public LobbyFormat Format { get; set; }
	public List<MultiplayerPlayer> PlayerList { get; } = new();
	public TextBox ChatLog { get; init; }

	public void UpdateState(BanchoBotDataParser banchoData)
	{
		throw new NotImplementedException();
	}
}

public class MultiplayerPlayer
{
	public MultiplayerPlayer(string name, TeamAssignment team)
	{
		Name = name;
		Team = team;
	}
	
	public string Name { get; init; }
	public TeamAssignment Team { get; set; }
}