namespace BrigittaBlazor.Settings;

public class UserAudioAlert
{
	public string Name { get; set; } = string.Empty;
	public string Path { get; set; } = string.Empty;
	public bool Enabled { get; set; } = true;
	public bool MultiplayerLobbySpecific { get; set; } = false;
	public double Volume { get; set; } = 0.5;
	public EventTrigger Trigger { get; set; }
	public string TriggerWord { get; set; } = string.Empty;

	public override string ToString() => $"{Name} => {Trigger}";
}

public enum EventTrigger
{
	OnMessage,
	OnDirectMessage,
	OnUsernameMentioned,
	OnKeyword
}