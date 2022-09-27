using Brigitta.Styles;
using ReactiveUI;

namespace Brigitta.ViewModels
{
	public class ViewModelBase : ReactiveObject
	{
		protected ViewModelBase() { Palette = new Palette(); }
		public Palette Palette { get; }
	}
}