using BrigittaBlazor.Settings;

namespace BrigittaBlazor.Utils;

/// <summary>
/// Singleton responsible for maintaining state in components
/// </summary>
public class StateMaintainer
{
	private readonly ILogger<StateMaintainer> _logger;

	public StateMaintainer(EventRegistrationTracker eventTracker, ILogger<StateMaintainer> logger)
	{
		_logger = logger;
		EventTracker = eventTracker;
		ChannelNotifications = new Dictionary<string, ChatNotification>();

		OnAudioAlertCreated += (_, alert) =>
		{
			_logger.LogInformation($"Created audio alert: {alert}");
		};
		
		OnAudioAlertUpdated += (_, alert) =>
		{
			_logger.LogInformation($"Updated audio alert: {alert}");
		};
		
		OnAudioAlertDeleted += (_, alert) =>
		{
			_logger.LogInformation($"Deleted audio alert: {alert}");
		};
	}

	public List<UserAudioAlert> AudioAlerts { get; } = new();
	public event EventHandler<UserAudioAlert> OnAudioAlertCreated;
	public event EventHandler<UserAudioAlert> OnAudioAlertUpdated;
	public event EventHandler<UserAudioAlert> OnAudioAlertDeleted;
	
	public void AddAudioAlert(UserAudioAlert audioAlert)
	{
		AudioAlerts.Add(audioAlert);
		OnAudioAlertCreated?.Invoke(this, audioAlert);
	}

	public void UpdateAudioAlert(UserAudioAlert audioAlert)
	{
		var existingAlert = AudioAlerts.FirstOrDefault(a => a.Name == audioAlert.Name);
		if (existingAlert != null)
		{
			AudioAlerts.Remove(existingAlert);
			AudioAlerts.Add(audioAlert);
			OnAudioAlertUpdated?.Invoke(this, audioAlert);
		}
	}

	public void DeleteAudioAlert(UserAudioAlert audioAlert)
	{
		if (AudioAlerts.Remove(audioAlert))
		{
			OnAudioAlertDeleted?.Invoke(this, audioAlert);
		}
	}
	
	// TODO: Add state such as Referee-view timer settings, etc.
	
	public EventRegistrationTracker EventTracker { get; }
	public Dictionary<string, ChatNotification> ChannelNotifications { get; }
}