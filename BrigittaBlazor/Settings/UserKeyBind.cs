using System.Text;

namespace BrigittaBlazor.Settings;

public class UserKeyBind
{
	public string Key { get; set; }
	public bool Alt { get; set; }
	public bool Ctrl { get; set; }
	public bool Shift { get; set; }
	/// <summary>
	///  The message sent to chat when the key combination gets pressed
	/// </summary>
	public string Message { get; set; }

	public override string ToString()
	{
		var sb = new StringBuilder();
		if (Ctrl)
		{
			sb.Append("CTRL+");
		}

		if (Shift)
		{
			sb.Append("SHIFT+");
		}

		if (Alt)
		{
			sb.Append("ALT+");
		}

		sb.Append(Key.ToUpper().Replace("CONTROL", "").Replace("ALT", "").Replace("SHIFT", ""));
		return sb.ToString();
	}
}