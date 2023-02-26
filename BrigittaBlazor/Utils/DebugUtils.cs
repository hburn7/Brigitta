namespace BrigittaBlazor.Utils;

public class DebugUtils
{
	public static bool IsDebugBuild()
	{
#if DEBUG
		return true;
#else
		return false;
#endif
	}
}