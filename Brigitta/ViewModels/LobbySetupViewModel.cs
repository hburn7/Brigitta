using Brigitta.Models.Irc;
using NLog;
using ReactiveUI;
using System.Collections.Generic;

namespace Brigitta.ViewModels;

public class LobbySetupViewModel : ViewModelBase
{
	private readonly IrcWrapper _irc;
	private readonly Logger _logger;
	
	private string _name;
	private int _size;
	
	private int? _scoreBlue;
	private int? _scoreRed;

	public LobbySetupViewModel(IrcWrapper irc, string name)
	{
		_irc = irc;
		_logger = LogManager.GetCurrentClassLogger();
		
		_name = name;
		_size = 1;
	}

	// Parameterless constructor used only for design view
	public LobbySetupViewModel() : this(new IrcWrapper(), "foo") {}

	public string Name
	{
		get => _name;
		set => this.RaiseAndSetIfChanged(ref _name, value);
	}

	public int Size
	{
		get => _size;
		set => this.RaiseAndSetIfChanged(ref _size, value);
	}

	public int? ScoreBlue
	{
		get => _scoreBlue;
		set => this.RaiseAndSetIfChanged(ref _scoreBlue, value);
	}
	
	public int? ScoreRed
	{
		get => _scoreRed;
		set => this.RaiseAndSetIfChanged(ref _scoreRed, value);
	}

	public List<double> SizeTicks => new List<double> { 2, 4, 6, 8, 10, 12, 14, 16 };
	
	public void Submit()
	{
		// todo: implement submit button
		_logger.Info($"Lobby information updated. Name={Name} Size={Size} Format=?");
		
	}
}