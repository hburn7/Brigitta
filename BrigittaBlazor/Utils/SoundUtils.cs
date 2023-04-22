using Microsoft.JSInterop;

namespace BrigittaBlazor.Utils;

public static class SoundUtils
{
	public static async ValueTask PlaySoundAsync(IJSRuntime jsRuntime, string soundFile)
	{
		await jsRuntime.InvokeVoidAsync("playSound", "Sounds/" + soundFile);
	}
}