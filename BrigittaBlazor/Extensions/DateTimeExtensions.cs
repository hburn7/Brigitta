using System;

namespace Brigitta.Extensions;

public static class DateTimeExtensions
{
	public static string ToFormattedTimeString(this DateTime time) => $"{time:hh:mm:ss}";
}