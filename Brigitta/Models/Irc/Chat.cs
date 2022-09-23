using IrcDotNet;
using System;
using System.Linq;
using System.Text;

namespace Brigitta.Models.Irc;

/// <summary>
/// Class containing all referenced IRC status codes.
/// </summary>
public static class IrcCodes
{
	public const int Greeting = 001;
	public const int MessageOfTheDay = 372;
	public const int LoginError = 464;

	public const string PrivateMessage = "PRIVMSG";
	public const string Quit = "QUIT";
	public const string Query = "QUERY";
	public const string Join = "JOIN";
	public const string Part = "PART";
	public const string Ping = "PING";
	public const string Me = "ME";
	public const string Mode = "MODE";
	public const string Replaced = "REPLACED";
}

public class IrcCommand
{
	public string Command { get; }
	
	public IrcCommand(string command) { Command = command; }

	/// <summary>
	/// Whether the IrcCommand is of the provided status code
	/// </summary>
	/// <param name="ircCode">The status code. Refer to integer <see cref="IrcCodes"/></param>
	/// <returns></returns>
	public bool IsStatus(int ircCode) => int.TryParse(Command, out int code) && ircCode == code;

	public override string ToString() => Command;
}

public class ChatMessage
{
	private readonly IrcClient.IrcMessage _ircMessage;

	public ChatMessage(IrcClient.IrcMessage ircMessage)
	{
		_ircMessage = ircMessage;
		
		// Properties
		IrcCommand = new IrcCommand(ircMessage.Command);
		TimeStamp = DateTime.Now;
		Content = GetContent();
		Sender = IdentifySender();
		Recipient = _ircMessage.Parameters[0];
		TimeStampPrint = FormattedDateTime(TimeStamp);
	}

	// Mainly for testing, but still viable
	public ChatMessage(DateTime timeStamp, IrcCommand ircCommand, string? content, string? sender, string recipient)
	{
		TimeStamp = timeStamp;
		TimeStampPrint = FormattedDateTime(timeStamp);
		IrcCommand = ircCommand;
		Content = content;
		Sender = sender;
		Recipient = recipient;
	}
	
	// Mainly for testing, but still viable
	public ChatMessage(IrcCommand ircCommand, string? content, string? sender, string recipient)
	{
		TimeStamp = DateTime.Now;
		TimeStampPrint = FormattedDateTime(TimeStamp);
		IrcCommand = ircCommand;
		Content = content;
		Sender = sender;
		Recipient = recipient;
	}

	private string FormattedDateTime(DateTime timeStamp) => $"{timeStamp:hh:mm:ss}";
	
	/// <summary>
	/// The <see cref="DateTime"/> at which this chat message was created
	/// </summary>
	public DateTime TimeStamp { get; }
	/// <summary>
	/// Formatted TimeStamp
	/// </summary>
	public string TimeStampPrint { get; }
	/// <summary>
	/// The command associated with this message
	/// </summary>
	public IrcCommand IrcCommand { get; }
	/// <summary>
	/// The content of the message
	/// </summary>
	public string? Content { get; }
	/// <summary>
	/// The sender of the message
	/// </summary>
	public string? Sender { get; }
	/// <summary>
	/// The channel or user the message is being sent tos
	/// </summary>
	public string Recipient { get; }
	
	public override string ToString() => $"{TimeStampPrint} [{IrcCommand}] {Sender} -> {Recipient}: {Content}";

	/// <summary>
	/// The chat message formatted for console display
	/// </summary>
	public string ConsoleLog() => $"{TimeStampPrint} {Sender}: {Content}";

	/// <summary>
	/// The chat message formatted for console display, with a new line appended
	/// </summary>
	public string ConsoleLogLine() => new StringBuilder()
	                                  .AppendLine(ConsoleLog())
	                                  .ToString();

	private string GetContent()
	{
		if (IrcCommand.Command == IrcCodes.PrivateMessage && (!_ircMessage.Source?.Name?.Equals("cho.ppy.sh") ?? false))
		{
			return _ircMessage.Parameters[1];
		}

		return string.Join(" ", _ircMessage.Parameters.ToArray()[1..]).Trim();
	}
	
	private string? IdentifySender() => IrcCommand.Command == IrcCodes.PrivateMessage ? _ircMessage.Source?.Name : null;
}