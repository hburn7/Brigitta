using BanchoSharp.Messaging;

namespace BrigittaBlazor.Derivatives;

public struct CustomCommand
{
	public string Command { get; set; }
	public string[]? Aliases { get; set; }
	public CustomParameter[]? Parameters { get; set; }
	public string Description { get; set; }
	
	public Delegate Function { get; set; }
	/// <summary>
	/// Executes <see cref="Function"/> with <see cref="Arguments"/> dynamically and returns the result.
	/// This allows us to invoke a function without knowledge of the parameters or method body. This is the equivalent
	/// of a generic Func delegate with a return type.
	/// </summary>
	/// <typeparam name="TResult">The return type of the delegate, must be <see cref="IConvertible"/></typeparam>
	/// <returns>The same return value from the delegate</returns>
	public TResult Execute<TResult>(Delegate function, params object?[] args) where TResult : IConvertible
	{
		object result = function.DynamicInvoke(args);
		return (TResult)Convert.ChangeType(result, typeof(TResult));
	}

	/// <summary>
	/// Executes a <see cref="Function"/> with <see cref="Arguments"/> dynamically as a <see cref="Task"/>.
	/// This allows us to invoke a function without knowledge of the parameters or method body. This is the equivalent
	/// of a generic Func delegate with as many parameters as necessary.
	/// </summary>
	/// <typeparam name="TResult">The return type of the delegate</typeparam>
	/// <returns>The same return value from the delegate</returns>
	public Task Execute(Delegate function, params object?[] args) => Task.Run(() => function.DynamicInvoke(args));

	/// <summary>
	/// True if the command the user is sending corresponds to this custom command.
	/// </summary>
	public bool IsAssociated(string command) => Command.Equals(command, StringComparison.OrdinalIgnoreCase) || 
	                                      Aliases?.Any(a => a.Equals(command, StringComparison.OrdinalIgnoreCase)) == true;
}

public struct CustomParameter
{
	public string Name { get; set; }
	public string Description { get; set; }
	public bool Optional { get; set; }
}

public class CustomCommandHandler : SlashCommandHandler
{
	public CustomCommandHandler(string prompt) : base(prompt)
	{
		if (string.IsNullOrEmpty(Command))
		{
			return;
		}

		if (ClearCommand.IsAssociated(Command))
		{
			CustomCommand = ClearCommand;
		}
		else if (ChatCommand.IsAssociated(Command))
		{
			CustomCommand = ChatCommand;
		}
		else if (TimerCommand.IsAssociated(Command))
		{
			CustomCommand = TimerCommand;
		}
		else if (MatchStartTimerCommand.IsAssociated(Command))
		{
			CustomCommand = MatchStartTimerCommand;
		}
	}

	/// <summary>
	/// The custom command, if applicable, that is associated with the user's prompt.
	/// Null if no custom command could be found from the prompt. Otherwise, it will
	/// contain a fully populated <see cref="CustomCommand"/> object that then can be acted upon.
	/// </summary>
	public CustomCommand? CustomCommand { get; set; }

	private readonly CustomCommand ClearCommand = new()
	{
		Command = "clear",
		Description = "Clears the chat"
	};

	private readonly CustomCommand ChatCommand = new()
	{
		Command = "chat",
		Aliases = new[] { "c" },
		Description = "Chat a user or channel, opening a line of communication with them (if not present) " +
		              "and optionally sending a message to them.",
		Parameters = new[]
		{
			new CustomParameter
			{
				Name = "user",
				Description = "The user to chat",
				Optional = false
			},
			new CustomParameter
			{
				Name = "message",
				Description = "The message to send",
				Optional = true
			}
		}
	};
	
	private readonly CustomCommand TimerCommand = new()
	{
		Command = "timer",
		Aliases = new[] { "t" },
		Description = "Starts a standard timer for a specified amount of time.",
		Parameters = new[]
		{
			new CustomParameter
			{
				Name = "time",
				Description = "The number of seconds the timer will last",
				Optional = false
			}
		}
	};

	private readonly CustomCommand MatchStartTimerCommand = new()
	{
		Command = "matchtimer",
		Aliases = new[] { "mt", "mst" },
		Description = "Starts a match start timer for the current lobby.",
		Parameters = new[]
		{
			new CustomParameter
			{
				Name = "time",
				Description = "The number of seconds the timer will last",
				Optional = false
			}
		}
	};
}

