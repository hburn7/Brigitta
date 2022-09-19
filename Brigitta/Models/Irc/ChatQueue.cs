using NLog;
using System;
using System.Collections.Concurrent;
using System.Linq;

// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

namespace Brigitta.Models.Irc;

public class ChatQueue : ConcurrentQueue<ChatMessage>
{
	private readonly Logger _logger;

	public event Action<ChatMessage>? OnEnqueue; 

	public ChatQueue()
	{
		_logger = LogManager.GetCurrentClassLogger();
	}

	public new void Enqueue(ChatMessage chatMessage)
	{
		string[] invalid = { IrcCodes.Quit, IrcCodes.Part, IrcCodes.Join, IrcCodes.Replaced };
		
		if (!invalid.Contains(chatMessage.IrcCommand.Command))
		{
			base.Enqueue(chatMessage);
			_logger.Trace($"Message enqueued: {chatMessage}");
			OnEnqueue?.Invoke(chatMessage);
		}
	}
}