using Microsoft.JSInterop;
using System.Text.Json;

namespace BrigittaBlazor.Utils;

public static class HotkeyListener
{
	public static event Action<object> OnHotkeyPressed;
	
	[JSInvokable]
	public static Task OnKeyDown(JsonElement eventObject)
	{
		OnHotkeyPressed?.Invoke(eventObject);
		return Task.CompletedTask;
	}
}