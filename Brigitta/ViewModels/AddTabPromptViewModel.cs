using ReactiveUI;
using System;

namespace Brigitta.ViewModels;

public class AddTabPromptViewModel : ViewModelBase
{
	private string? _tabName;

	public string? TabName
	{
		get => _tabName; 
		set => this.RaiseAndSetIfChanged(ref _tabName, value);
	}
}