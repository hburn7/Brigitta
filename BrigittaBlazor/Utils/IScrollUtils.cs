namespace BrigittaBlazor.Utils;

public interface IScrollUtils
{
	public Task ScrollToBottomAsync(string divId = "console", int? delayMs = 5);
}