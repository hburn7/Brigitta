using Avalonia.Media;

namespace Brigitta.Styles;

public class Palette
{
	public Palette()
	{
#region CtorFields
		FieldDefaultBorderBrush = new SolidColorBrush(Color.Parse("#99ffffff"));
		FieldErrorBorderBrush = new SolidColorBrush(Colors.Red);
#endregion CtorFields

		DefaultBackgroundBrush = new SolidColorBrush(Color.Parse("#575360"));
		DarkBackgroundBrush = new SolidColorBrush(Color.Parse("#232126"));

		TeamBlueBackgroundBrush = new SolidColorBrush(Color.Parse("#1f42dd"));
		TeamRedBackgroundBrush = new SolidColorBrush(Color.Parse("#e51919"));
#region CtorButtons
		ButtonSubmitBrush = new SolidColorBrush(Color.Parse("#339933"));
		ButtonInformationBrush = new SolidColorBrush(Color.Parse("#27b5c2"));
		ButtonCautionBrush = new SolidColorBrush(Color.Parse("#c4c730"));
		ButtonDangerBrush = new SolidColorBrush(Color.Parse("#d12c2c"));
#endregion CtorButtons
	}

	public IBrush DefaultBackgroundBrush { get; }
	public IBrush DarkBackgroundBrush { get; }
	public IBrush TeamRedBackgroundBrush { get; }
	public IBrush TeamBlueBackgroundBrush { get; }

#region Buttons
	public IBrush ButtonSubmitBrush { get; }
	public IBrush ButtonInformationBrush { get; }
	public IBrush ButtonCautionBrush { get; }
	public IBrush ButtonDangerBrush { get; }
#endregion

#region Fields
	public IBrush FieldDefaultBorderBrush { get; }
	public IBrush FieldErrorBorderBrush { get; }
#endregion
}