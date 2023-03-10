namespace BrigittaBlazor.Utils;

[Flags]
public enum ChatNotification
{
	CurrentlySelected = 1 << 0,
	MentionsRefereeKeywords = 1 << 1,
	DirectMessage = 1 << 2,
	MentionsUsername = 1 << 3,
	GeneralMessage = 1 << 4,
	None = 0
}