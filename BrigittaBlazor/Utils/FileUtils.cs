﻿using BanchoSharp.Interfaces;
using BrigittaBlazor.Extensions;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text;

namespace BrigittaBlazor.Utils;

public static class FileUtils
{
	/// <summary>
	///  Downloads a file to the browser
	/// </summary>
	/// <param name="js"></param>
	/// <param name="filename">The name of the file with extension</param>
	/// <param name="data"></param>
	public static async Task DownloadAs(IJSRuntime js, string filename, byte[] data) => await js.InvokeVoidAsync(
		"saveAsFile",
		filename,
		Convert.ToBase64String(data));

	public static async Task SaveChatHistoryAsync(ILogger logger, IChatChannel? currentChannel, bool displayUTC, IJSRuntime js,
		ISnackbar snackbar)
	{
		if (currentChannel == null)
		{
			snackbar.Add("Cannot download messages, channel is null.", Severity.Error);
			logger.LogDebug("User attempted a /savelog, but the current channel was null");
			return;
		}

		if (currentChannel.MessageHistory == null)
		{
			snackbar.Add("Cannot download messages, message history is null.", Severity.Error);
			logger.LogDebug($"User attempted a /savelog, but the message history for {currentChannel.ChannelName} was null");
			return;
		}

		if (!currentChannel.MessageHistory.Any())
		{
			snackbar.Add("There are no chat messages in this channel to save.", Severity.Error);
			logger.LogDebug($"User attempted a /savelog, but no messages were found in {currentChannel.ChannelName}");
			return;
		}

		var first = currentChannel.MessageHistory.First?.Value;
		var last = currentChannel.MessageHistory.Last?.Value;

		if (first == null || last == null)
		{
			snackbar.Add("Error occurred when saving chat log.", Severity.Error);
			logger.LogDebug($"User attempted a /savelog, but the first or last message was null in {currentChannel.ChannelName}");
			return;
		}

		var timeDelta = last.Timestamp - first.Timestamp;

		var sb = new StringBuilder($"---- Chat log of {currentChannel.ChannelName} generated by Brigitta ----\n");
		var ts = displayUTC ? currentChannel.MessageHistory!.First!.Value.Timestamp.ToUniversalTime() : currentChannel.MessageHistory!.First!.Value.Timestamp;
		sb.AppendLine($"---- Log spans {timeDelta.Humanize(3, minUnit: TimeUnit.Second)}, " +
		              $"beginning at {ts:R}{ts:zz} ----");

		sb.AppendLine("---- BEGIN LOG ----");
		foreach (var ircMessage in currentChannel.MessageHistory)
		{
			if (ircMessage is not IPrivateIrcMessage message)
			{
				continue;
			}

			if (displayUTC)
			{
				sb.AppendLine(message.ToUTCDisplayString());
			}
			else
			{
				sb.AppendLine(message.ToDisplayString());
			}
		}

		sb.AppendLine("---- END LOG ----");

		await DownloadAs(js, $"{DateTime.Now.ToUniversalTime().ToFileTimeString()}-{currentChannel.ChannelName}.txt",
			Encoding.UTF8.GetBytes(sb.ToString()));

		snackbar.Add($"Downloaded chat log for {currentChannel.ChannelName}.", Severity.Success);
		logger.LogDebug($"Saved chat log for {currentChannel.ChannelName}");
	}
	
	public static string ExtractFilename(string path)
	{
		var fInfo = new FileInfo(path);
		return fInfo.Name;
	}
}