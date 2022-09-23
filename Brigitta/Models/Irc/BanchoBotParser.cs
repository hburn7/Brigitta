using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// Null check for chatmessage content-- we assert it's not null in constructor
#pragma warning disable CS8604

namespace Brigitta.Models.Irc;

public enum LobbyFormat
{
	HeadToHead,
	TagCoop,
	TeamVs,
	TagTeamVs
}

public enum WinCondition
{
	Score,
	ScoreV2,
	Accuracy,
	Combo
}

public enum GameMode
{
	osu,
	osuMania,
	osuTaiko,
	osuCatch
}

public class LobbyData
{
	public string? Name { get; set; }
	public Uri? History { get; set; }
	public int? Size { get; set; }
	public LobbyFormat? Format { get; set; }
	public WinCondition? WinCondition { get; set; }
	public GameMode? GameMode { get; set; }
}

public class LobbyCreationInfo
{
	private static readonly Regex _historyRegex = new(@"https:\/\/osu.ppy.sh\/mp\/[0-9]{7,}");

	public LobbyCreationInfo(string content)
	{
		History = new Uri(_historyRegex.Match(content).Value, UriKind.Absolute);
		Name = content.Split(History.AbsoluteUri).Last().Trim();
	}

	public string Name { get; set; }
	public Uri History { get; set; }
}

public class BanchoBotDataParser
{
	private static readonly Dictionary<string, LobbyFormat> _formatMap = new()
	{
		{ "HeadToHead", LobbyFormat.HeadToHead },
		{ "TagCoop", LobbyFormat.TagCoop },
		{ "TeamVs", LobbyFormat.TeamVs },
		{ "TagTeamVs", LobbyFormat.TagTeamVs }
	};
	private static readonly Dictionary<string, WinCondition> _winConditionMap = new()
	{
		{ "Score", WinCondition.Score },
		{ "Accuracy", WinCondition.Accuracy },
		{ "Combo", WinCondition.Combo },
		{ "ScoreV2", WinCondition.ScoreV2 }
	};
	private static readonly Dictionary<string, GameMode> _gameModeMap = new()
	{
		{ "Osu", GameMode.osu },
		{ "Taiko", GameMode.osuTaiko },
		{ "CatchTheBeat", GameMode.osuCatch },
		{ "OsuMania", GameMode.osuMania }
	};

	// Regexes
	private static readonly Regex _historyRegex = new(@"https:\/\/osu.ppy.sh\/mp\/[0-9]{7,}");
	private static readonly Regex _playerCountRegex = new(@"Players: [0-9]{1}[0-6]?");
	private readonly ChatMessage _chatMessage;

	public BanchoBotDataParser(ChatMessage chatMessage)
	{
		if (chatMessage.Sender != "BanchoBot")
		{
			throw new InvalidOperationException("Cannot parse message not from BanchoBot");
		}

		if (chatMessage.Content == null)
		{
			throw new InvalidOperationException("A message from BanchoBot should never have null content");
		}

		_chatMessage = chatMessage;

		ParsedData = new LobbyData
		{
			Name = ExtractName(),
			History = ExtractHistory(),
			Size = ExtractSize(),
			Format = ExtractFormat(),
			WinCondition = ExtractWinCondition(),
			GameMode = ExtractGameMode()
		};

		IsCreationResponse = chatMessage.Content.Contains("Created the tournament match");
		CreationInfo = IsCreationResponse ? new LobbyCreationInfo(chatMessage.Content) : null;
	}

	/// <summary>
	///  True if the message is a direct response to a "!mp make" command
	/// </summary>
	public bool IsCreationResponse { get; }
	public LobbyCreationInfo? CreationInfo { get; }
	public LobbyData ParsedData { get; }

	private Uri? ExtractHistory()
	{
		if (_historyRegex.IsMatch(_chatMessage.Content))
		{
			return new Uri(_historyRegex.Match(_chatMessage.Content).Value, UriKind.Absolute);
		}

		return null;
	}

	private string? ExtractName()
	{
		if (_chatMessage.Content!.Contains("Room name:"))
		{
			// Format:
			// Room name: BrigittaTest, History: https://osu.ppy.sh/12345
			string[] splits = _chatMessage.Content!.Split("Room name:");
			return splits[1].Split(", History:")[0].Trim();
		}

		return null;
	}

	private int? ExtractSize()
	{
		if (_playerCountRegex.IsMatch(_chatMessage.Content))
		{
			if (_chatMessage.Content.Length == 11 && int.TryParse(_chatMessage.Content[10].ToString(), out int trailingNum))
			{
				// This would be the case if the message content was
				// Players: 17, 18, etc.

				// Probably not even needed considering it's currently impossible
				// to set the lobby > 16
				if (trailingNum > 6)
				{
					throw new InvalidOperationException("Lobby size is greater than 16");
				}
			}

			string[] splits = _chatMessage.Content.Split(" ");
			string toParse = splits.Last();
			if (int.TryParse(toParse, out int size))
			{
				return size;
			}
		}

		return null;
	}

	private LobbyFormat? ExtractFormat()
	{
		// Team mode: HeadToHead, Win condition: Score
		if (_chatMessage.Content!.Contains("Team mode: "))
		{
			string[] splits = _chatMessage.Content.Split(", ");
			return _formatMap[splits[0].Split(":")[1].Trim()];
		}

		return null;
	}

	private WinCondition? ExtractWinCondition()
	{
		if (_chatMessage.Content!.Contains(", Win condition: "))
		{
			string[] splits = _chatMessage.Content.Split(" ");
			return _winConditionMap[splits.Last().Trim()];
		}

		return null;
	}

	private GameMode? ExtractGameMode()
	{
		if (_chatMessage.Content!.Contains("Changed match mode to "))
		{
			string mode = _chatMessage.Content!.Split(" ").Last();
			return _gameModeMap[mode];
		}

		return null;
	}

	public bool IsBanchoBotMessage() => _chatMessage.Sender == "BanchoBot";
}