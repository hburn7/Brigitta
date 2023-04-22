using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Text.Json;

#pragma warning disable CS8618

namespace BrigittaBlazor.Utils;

public static class HotkeyListener
{
	public static event Action<ParsedJsonEvent> OnHotkeyPressed;

	[JSInvokable]
	public static Task OnKeyDown(JsonElement eventObject)
	{
		var parsed = JsonConvert.DeserializeObject<ParsedJsonEvent>(eventObject.ToString());
		if (parsed != null)
		{
			OnHotkeyPressed?.Invoke(parsed);
		}

		return Task.CompletedTask;
	}
}

public class CurrentTarget
{
	[JsonProperty("location")]
	public Location Location { get; set; }
}

public class Location
{
	[JsonProperty("href")]
	public string Href { get; set; }
	[JsonProperty("origin")]
	public string Origin { get; set; }
	[JsonProperty("protocol")]
	public string Protocol { get; set; }
	[JsonProperty("host")]
	public string Host { get; set; }
	[JsonProperty("hostname")]
	public string Hostname { get; set; }
	[JsonProperty("port")]
	public string Port { get; set; }
	[JsonProperty("pathname")]
	public string Pathname { get; set; }
	[JsonProperty("search")]
	public string Search { get; set; }
	[JsonProperty("hash")]
	public string Hash { get; set; }
}

public class ParsedJsonEvent
{
	[JsonProperty("altKey")]
	public bool AltKey { get; set; }
	[JsonProperty("bubbles")]
	public bool Bubbles { get; set; }
	[JsonProperty("cancelBubble")]
	public bool CancelBubble { get; set; }
	[JsonProperty("cancelable")]
	public bool Cancelable { get; set; }
	[JsonProperty("charCode")]
	public int CharCode { get; set; }
	[JsonProperty("code")]
	public string Code { get; set; }
	[JsonProperty("composed")]
	public bool Composed { get; set; }
	[JsonProperty("ctrlKey")]
	public bool CtrlKey { get; set; }
	[JsonProperty("currentTarget")]
	public CurrentTarget CurrentTarget { get; set; }
	[JsonProperty("defaultPrevented")]
	public bool DefaultPrevented { get; set; }
	[JsonProperty("detail")]
	public int Detail { get; set; }
	[JsonProperty("eventPhase")]
	public int EventPhase { get; set; }
	[JsonProperty("isComposing")]
	public bool IsComposing { get; set; }
	[JsonProperty("isTrusted")]
	public bool IsTrusted { get; set; }
	[JsonProperty("key")]
	public string Key { get; set; }
	[JsonProperty("keyCode")]
	public int KeyCode { get; set; }
	[JsonProperty("location")]
	public int Location { get; set; }
	[JsonProperty("metaKey")]
	public bool MetaKey { get; set; }
	[JsonProperty("repeat")]
	public bool Repeat { get; set; }
	[JsonProperty("returnValue")]
	public bool ReturnValue { get; set; }
	[JsonProperty("shiftKey")]
	public bool ShiftKey { get; set; }
	[JsonProperty("target")]
	public object Target { get; set; }
	[JsonProperty("timeStamp")]
	public int TimeStamp { get; set; }
	[JsonProperty("type")]
	public string Type { get; set; }
	[JsonProperty("which")]
	public int Which { get; set; }
}