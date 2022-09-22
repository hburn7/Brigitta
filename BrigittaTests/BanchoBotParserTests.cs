using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BrigittaTests;

public class BanchoBotParserTests
{
	private static readonly string[] _testNames =
	{
		"BrigittaTest", "B r i g i t t a t e s t",
		"1", "1234567890", "!@ e23er3239ry2387fhy3y    87d9  YHD& EYF*&YF9 *&f742t92 y",
		"~`!@#$%^&*()_+1234567890-=qwertyuiop[]\\asdfghjkl;'zxcvbnm,./<>?:_+QWERTYUIOPASDFGHJKLZXCVBNM<{}\"'",
		"バカ", "this,name, has, commas, and , spaces, !", ""
	};

	private static readonly string _testHistoryUrl = "https://osu.ppy.sh/mp/103846117";
	private static readonly LobbyFormat[] _allFormats =
	{
		LobbyFormat.TeamVs, LobbyFormat.HeadToHead, LobbyFormat.TagCoop, LobbyFormat.TagTeamVs
	};

	private static readonly WinCondition[] _allWinConditions =
	{
		WinCondition.Score, WinCondition.ScoreV2, WinCondition.Accuracy, WinCondition.Combo
	};
	
	/**
	 * Stage
	    !mp settings
	    BanchoBot
	    Room name: BrigittaTest, History: https://osu.ppy.sh/mp/103846117
	    BanchoBot
	    Team mode: HeadToHead, Win condition: Score
	    BanchoBot
	    Players: 0
	 */

	[SetUp]
	public void Setup() {}

	/**
		Stage     - !mp make BrigittaTest
	    BanchoBot - Created the tournament match https://osu.ppy.sh/mp/103846117 BrigittaTest
	 */
	[Test]
	public void CreateTournamentMatch()
	{
		var tournamentCreatedMessage = new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage),
			$"Created the tournament match {_testHistoryUrl} BrigittaTest",
			"BanchoBot", "Stage");

		var parser = new BanchoBotDataParser(tournamentCreatedMessage);
		Assert.True(parser.IsCreationResponse);
	}

	[Test]
	public void MpSettingsDataExtraction()
	{
		// !mp settings responses come in waves of 3.
		foreach(string name in _testNames)
		{
			// Name, history
			var res1 = new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage),
				$"Room name: {name}, History: {_testHistoryUrl}", "BanchoBot", "Stage");

			var parser = new BanchoBotDataParser(res1);
			Assert.That(name, Is.EqualTo(parser.ParsedData.Name));
			Assert.That(_testHistoryUrl, Is.EqualTo(parser.ParsedData.History));
		}

		foreach (var format in _allFormats)
		{
			foreach (var condition in _allWinConditions)
			{
				// Team mode, win condition
				var res2 = new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage),
					$"Team mode: {format}, Win condition: {condition}", "BanchoBot", "Stage");

				var parser = new BanchoBotDataParser(res2);
				Assert.That(format, Is.EqualTo(parser.ParsedData.Format));
				Assert.That(condition, Is.EqualTo(parser.ParsedData.WinCondition));
			}
		}

		for (int i = 0; i <= 16; i++)
		{
			// Players
			var res3 = new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage),
				$"Players: {i}", "BanchoBot", "Stage");

			var parser = new BanchoBotDataParser(res3);
			
			Assert.That(i, Is.EqualTo(parser.ParsedData.Size));
		}
	}

	[Test]
	public void TournamentCreationDataExtraction()
	{
		foreach(string name in _testNames)
        {
            var tournamentCreatedMessage = new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage),
				$"Created the tournament match {_testHistoryUrl} {name}",
				"BanchoBot", "Stage");

			var parser = new BanchoBotDataParser(tournamentCreatedMessage);
            Assert.Multiple(() =>
            {
                Assert.True(parser.IsCreationResponse);

                Assert.That(tournamentCreatedMessage.Content, Is.EqualTo($"Created the tournament match {_testHistoryUrl} {name}"));
            });
            var creationData = new LobbyCreationInfo(tournamentCreatedMessage.Content!);
			Assert.That(creationData.Name, Is.EqualTo(name));
			Assert.That(creationData.History, Is.EqualTo(_testHistoryUrl));
        }
    }

	[Test]
	public void ExceptionIfNotFromBanchoBot()
	{
		var message = new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage), "Hellosu!", "Stage", "Peppy");
		Assert.Throws<InvalidOperationException>(() =>
		{
			var _ = new BanchoBotDataParser(message);
		});
	}
	
	[Test]
	public void ExceptionIfNullContent()
	{
		var message = new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage),
			null, "BanchoBot", "Stage");

		Assert.Throws<InvalidOperationException>(() =>
		{
			var _ = new BanchoBotDataParser(message);
		});
	}

	[Test]
	public void TestHistoryIsAbsoluteUri()
	{
		bool isWellFormed = Uri.IsWellFormedUriString(_testHistoryUrl, UriKind.Absolute);
		
		Assert.That(isWellFormed, Is.True);

		var message = new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage), _testHistoryUrl, "BanchoBot", "Stage");
		var parser = new BanchoBotDataParser(message);
		
		Assert.That(Uri.IsWellFormedUriString(parser.ParsedData.History, UriKind.Absolute));
	}
}