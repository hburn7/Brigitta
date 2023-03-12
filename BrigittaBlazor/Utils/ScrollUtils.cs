using Microsoft.JSInterop;

namespace BrigittaBlazor.Utils;

public class ScrollUtils : IScrollUtils
{
	private readonly ILogger<ScrollUtils> _logger;
	private readonly IJSRuntime JS;

	public ScrollUtils(IJSRuntime js, ILogger<ScrollUtils> logger)
	{
		JS = js;
		_logger = logger;
	}

	public string ConsoleId => "console";
	public async Task ScrollToBottomAsync(string divId) => await JS.InvokeVoidAsync("scrollToBottom", divId);
}