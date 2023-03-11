namespace BrigittaBlazor.Utils;

public interface IScrollUtils
{
	public string ConsoleId { get; }
	public Task ScrollToBottomAsync(string divId);
}