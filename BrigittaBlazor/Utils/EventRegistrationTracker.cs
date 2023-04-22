namespace BrigittaBlazor.Utils;

public class EventRegistrationTracker
{
	public bool HasRegisteredSettingsHotkeyListener { get; set; }
	public bool HasRegisteredPrimaryDisplayDefaultEvents { get; set; }
	public bool HasRegisteredIndexLocationListener { get; set; }
}