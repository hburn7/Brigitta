using Brigitta.Models.Irc;
using System;
using System.Diagnostics;

namespace Brigitta.Extensions;

public static class IrcClientExtensions
{
	public static void SendMessage(this IrcWrapper wrapper, string channel, string message)
	{
#if DEBUG
		Debug.WriteLine($"Received SendMessage request: {channel}: \"{message}\"");
#endif

		if (wrapper.Client.IsConnected)
		{
			var cm = new ChatMessage(DateTime.Now, new IrcCommand(IrcCodes.PrivateMessage), message,
				wrapper.Credentials.Username, channel);

			wrapper.ChatQueue.Enqueue(cm);
			wrapper.Client.SendRawMessage($"PRIVMSG {channel} {message}");
		}
	}
}