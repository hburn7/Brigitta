using Brigitta.Styles;
using ReactiveUI;

namespace Brigitta.ViewModels
{
	public class ViewModelBase : ReactiveObject
	{
		public Palette Palette { get; }

		protected ViewModelBase()
		{
			Palette = new Palette();
		}
	}
}