using BanchoSharp.Interfaces;
using BrigittaBlazor.Extensions;

namespace Brigitta.Extensions;

public static class IrcMessageExtensions
{
	public static string ToDisplayString(this IIrcMessage m) => 
		$"{m.Timestamp.ToFormattedTimeString()} {m.Prefix}: {string.Join(" ", m.Parameters)}";

	public static string ToDisplayString(this IPrivateIrcMessage pm) => 
		$"{pm.Timestamp.ToFormattedTimeString()} {pm.Sender}: {pm.Content}";
	
	public static string ToLogString(this IPrivateIrcMessage pm) => 
		$"{pm.Timestamp.ToFormattedTimeString()} {pm.Sender} -> {pm.Recipient}: {pm.Content}";
	
	public static string ToTimeString(this IIrcMessage m) => 
		$"{m.Timestamp.ToFormattedTimeString()}";
	
	public static string ToUTCTimeString(this IIrcMessage m) =>
		$"{m.Timestamp.ToUniversalTime().ToFormattedTimeString()}";
}