using System;

namespace Brigitta.Extensions;

public static class DateTimeExtensions
{
	public static string ToFormattedTimeString(this DateTime time) => $"{time:HH:mm:ss}";
	public static string ToFormattedDuration(this TimeSpan? ts) => $"{ts:mm\\:ss}";
}