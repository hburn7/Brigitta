using Avalonia.Controls;
using Brigitta.Models.Irc;
using NLog;
using System;
using System.Collections.Generic;

namespace Brigitta.Models;

public enum TeamAssignment
{
	Red,
	Blue,
	None
}

// Maybe add !mp addref tracking as well
public class MultiplayerLobby
{
	private readonly Logger _logger;

	public MultiplayerLobby(string name, Uri history, LobbyFormat format, TextBox chatLog) : this()
	{
		Name = name;
		History = history;
		Format = format;
		ChatLog = chatLog;
	}

	public MultiplayerLobby() { _logger = LogManager.GetCurrentClassLogger(); }
	public string Name { get; private set; }
	public Uri History { get; private set; }
	public int Size { get; private set; }
	public GameMode GameMode { get; private set; }
	public LobbyFormat Format { get; private set; }
	public WinCondition WinCondition { get; private set; }
	public List<MultiplayerPlayer> PlayerList { get; } = new();
	public TextBox ChatLog { get; }

	public void UpdateState(BanchoBotDataParser banchoData)
	{
		if (!banchoData.IsBanchoBotMessage())
		{
			_logger.Warn("Attempted to update state with message not from BanchoBot. " +
			             $"{banchoData}");

			return;
		}

		if (banchoData.ParsedData.Name != null)
		{
			Name = banchoData.ParsedData.Name;
		}

		if (banchoData.ParsedData.History != null)
		{
			History = banchoData.ParsedData.History;
		}

		if (banchoData.ParsedData.Format != null)
		{
			Format = banchoData.ParsedData.Format.Value;
		}

		if (banchoData.ParsedData.Size != null)
		{
			Size = banchoData.ParsedData.Size.Value;
		}

		if (banchoData.ParsedData.GameMode != null)
		{
			GameMode = banchoData.ParsedData.GameMode.Value;
		}

		if (banchoData.ParsedData.Format != null)
		{
			Format = banchoData.ParsedData.Format.Value;
		}

		if (banchoData.ParsedData.WinCondition != null)
		{
			WinCondition = banchoData.ParsedData.WinCondition.Value;
		}
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