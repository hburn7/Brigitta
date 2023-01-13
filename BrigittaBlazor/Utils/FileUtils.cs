using Microsoft.JSInterop;

namespace BrigittaBlazor.Utils;

public static class FileUtils
{
	/// <summary>
	/// Downloads a file to the browser
	/// </summary>
	/// <param name="js"></param>
	/// <param name="filename">The name of the file with extension</param>
	/// <param name="data"></param>
	public static async Task DownloadAs(IJSRuntime js, string filename, byte[] data) => await js.InvokeVoidAsync(
		"saveAsFile",
		filename,
		Convert.ToBase64String(data));
}