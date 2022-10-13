using BanchoSharp.Interfaces;

namespace Brigitta.Extensions;

public static class IrcMessageExtensions
{
	public static string ToDisplayString(this IIrcMessage m) => 
		$"{m.Timestamp.ToFormattedTimeString()} {m.Prefix}: {string.Join(" ", m.Parameters)}";

	public static string ToDisplayString(this IPrivateIrcMessage pm) => 
		$"{pm.Timestamp.ToFormattedTimeString()} {pm.Sender}: {pm.Content}";
	
	public static string ToLogString(this IPrivateIrcMessage pm) => 
		$"{pm.Timestamp.ToFormattedTimeString()} {pm.Sender} -> {pm.Recipient}: {pm.Content}";
}