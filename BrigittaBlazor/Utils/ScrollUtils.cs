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

	public async Task ScrollToBottomAsync(string divId = "console", int? delayMs = 5)
	{
		try
		{
			if (delayMs.HasValue)
			{
				await Task.Delay(delayMs.Value);
			}

			await JS.InvokeVoidAsync("scrollToBottom", divId);
		}
		catch (Exception e)
		{
			_logger.LogWarning($"Exception encountered while trying to scroll to bottom of chat: {e.Message}");
		}
	}
}