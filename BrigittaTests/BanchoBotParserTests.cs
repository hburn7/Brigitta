#pragma warning disable CS8618
namespace BrigittaTests;

public class BanchoBotParserTests
{
	public static string[] TestNames { get; } =
	{
		"BrigittaTest", "B r i g i t t a t e s t",
		"1", "1234567890", "!@ e23er3239ry2387fhy3y    87d9  YHD& EYF*&YF9 *&f742t92 y",
		"~`!@#$%^&*()_+1234567890-=qwertyuiop[]\\asdfghjkl;'zxcvbnm,./<>?:_+QWERTYUIOPASDFGHJKLZXCVBNM<{}\"'",
		"バカ", "this,name, has, commas, and , spaces, !", ""
	};
	public static string TestHistoryUrl { get; } = "https://osu.ppy.sh/mp/103846117";
	public static Uri TestHistoryUri { get; } = new Uri("https://osu.ppy.sh/mp/103846117", UriKind.Absolute);
	public static LobbyFormat[] AllFormats { get; } =
	{
		LobbyFormat.TeamVs, LobbyFormat.HeadToHead, LobbyFormat.TagCoop, LobbyFormat.TagTeamVs
	};
	public static WinCondition[] AllWinConditions { get; } =
	{
		WinCondition.Score, WinCondition.ScoreV2, WinCondition.Accuracy, WinCondition.Combo
	};
	private ChatMessage _tournamentCreated;

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
	public void Setup()
	{
		_tournamentCreated = new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage),
			$"Created the tournament match {TestHistoryUrl} BrigittaTest",
			"BanchoBot", "Stage");
	}

	/**
		Stage     - !mp make BrigittaTest
	    BanchoBot - Created the tournament match https://osu.ppy.sh/mp/103846117 BrigittaTest
	 */
	[Test]
	public void CreateTournamentMatch()
	{
		var tournamentCreatedMessage = new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage),
			$"Created the tournament match {TestHistoryUrl} BrigittaTest",
			"BanchoBot", "Stage");

		var parser = new BanchoBotDataParser(tournamentCreatedMessage);
		Assert.True(parser.IsCreationResponse);
	}

	[Test]
	public void MpSettingsDataExtraction()
	{
		// !mp settings responses come in waves of 3.
		foreach(string name in TestNames)
		{
			// Name, history
			var res1 = new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage),
				$"Room name: {name}, History: {TestHistoryUrl}", "BanchoBot", "Stage");

			var parser = new BanchoBotDataParser(res1);
			Assert.That(name, Is.EqualTo(parser.ParsedData.Name));
			Assert.That(TestHistoryUri, Is.EqualTo(parser.ParsedData.History));
		}

		foreach (var format in AllFormats)
		{
			foreach (var condition in AllWinConditions)
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
		foreach(string name in TestNames)
		{
            var parser = new BanchoBotDataParser(new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage), 
	            $"Created the tournament match {TestHistoryUrl} {name}", "BanchoBot", "Stage"));
            
            Assert.Multiple(() =>
            {
                Assert.That(parser.IsCreationResponse, Is.True);
                Assert.That(parser.CreationInfo.History, Is.EqualTo(TestHistoryUri));
                Assert.That(parser.CreationInfo.Name, Is.EqualTo(name));
            });
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
		bool isWellFormed = Uri.IsWellFormedUriString(TestHistoryUrl, UriKind.Absolute);
		
		Assert.That(isWellFormed, Is.True);

		var message = new ChatMessage(new IrcCommand(IrcCodes.PrivateMessage), TestHistoryUrl, "BanchoBot", "Stage");
		var parser = new BanchoBotDataParser(message);
	}

	[Test]
	public void TestLobbyCreationInfo()
	{
		var parser = new BanchoBotDataParser(_tournamentCreated);
		
		Assert.Multiple(() =>
		{
			Assert.That(parser.IsCreationResponse, Is.True);
			Assert.That(parser.CreationInfo, Is.Not.EqualTo(null));
			Assert.That(parser.CreationInfo.Name, Is.EqualTo("BrigittaTest"));
			Assert.That(parser.CreationInfo.History, Is.EqualTo(TestHistoryUri));
		});
	}
}