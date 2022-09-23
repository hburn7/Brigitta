namespace BrigittaTests;

public class MultiplayerLobbyTests
{
	private BanchoBotDataParser[] _nameParsers;
	private BanchoBotDataParser[][] _formatWcParserCollection;

	[SetUp]
	public void Setup()
	{
		var names = BanchoBotParserTests.TestNames;
		var formats = BanchoBotParserTests.AllFormats;
		var winConditions = BanchoBotParserTests.AllWinConditions;
		
		_nameParsers = new BanchoBotDataParser[names.Length];
		_formatWcParserCollection = new BanchoBotDataParser[formats.Length][];
		
		for (int i = 0; i < names.Length; i++)
		{
			_nameParsers[i] = new BanchoBotDataParser(new ChatMessage(
				new IrcCommand(IrcCodes.PrivateMessage), $"Room name: {names[i]}, History: https://osu.ppy.sh/mp/103846117",
				"BanchoBot", "Stage"));
		}

		for (int i = 0; i < _formatWcParserCollection.Length; i++)
		{
			_formatWcParserCollection[i] = new BanchoBotDataParser[winConditions.Length];
			for (int j = 0; j < formats.Length; j++)
			{
				_formatWcParserCollection[i][j] = new BanchoBotDataParser(new ChatMessage(
					new IrcCommand(IrcCodes.PrivateMessage), $"Team mode: {formats[i]}, Win condition: {winConditions[j]}", 
					"BanchoBot", "Stage"));
			}
		}
	}

	[Test]
	public void TestUpdateState()
	{
		var mp = new MultiplayerLobby();
		foreach (var nhParser in _nameParsers)
		{
			mp.UpdateState(nhParser);
			Assert.Multiple(() =>
			{
				Assert.That(mp.Name, Is.EqualTo(nhParser.ParsedData.Name));
				Assert.That(mp.History, Is.EqualTo(nhParser.ParsedData.History));
			});
		}

		foreach (var fwcParsers in _formatWcParserCollection)
		{
			foreach(var parser in fwcParsers)
			{
				mp.UpdateState(parser);
		
				Assert.Multiple(() =>
				{
					Assert.That(parser.ParsedData.Format, Is.EqualTo(mp.Format));
					Assert.That(parser.ParsedData.WinCondition, Is.EqualTo(mp.WinCondition));
				});
			}
		}
	}
}