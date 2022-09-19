using IrcDotNet;
using NLog;
using System.Diagnostics;

namespace Brigitta.Models.Irc;

public class IrcWrapper
{
	private const string Address = "irc.ppy.sh";
	private const int Port = 6667;
	private const int TimeoutMs = 7000;
	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public StandardIrcClient Client { get; }
	public Credentials Credentials { get; }
	public ChatQueue ChatQueue { get; }

	public IrcWrapper(Credentials credentials)
	{
		Client = new StandardIrcClient();
		ChatQueue = new ChatQueue();
		Credentials = credentials;

		Client.RawMessageReceived += (_, args) => ChatQueue.Enqueue(new ChatMessage(args.Message));
	}

	public IrcWrapper() : this(new Credentials().Load())
	{
		Client = new StandardIrcClient();
	}

	public bool Login()
	{
		_logger.Trace($"Attempting to login to {Address}:{Port}");

		if (Credentials.RememberMe)
		{
			Credentials.Save();
		}
		
		Client.Connect(Address, Port, false, new IrcUserRegistrationInfo
		{
			UserName = Credentials.Username,
			NickName = Credentials.Username,
			Password = Credentials.Password
		});

		var sw = new Stopwatch();
		sw.Start();
		
		bool success = false;
		bool rejected = false;

		Client.RawMessageReceived += LoginCheck;

		void LoginCheck(object? sender, IrcRawMessageEventArgs args)
		{
			var chat = new ChatMessage(args.Message);
			ChatQueue.Enqueue(chat);
			
			_logger.Trace($"Message received: {args.Message}");

			if(chat.IrcCommand.IsStatus(IrcCodes.LoginError))
			{
				rejected = true;
				ChatQueue.Clear();
			}
			else if (!(chat.IrcCommand.IsStatus(IrcCodes.Greeting) || chat.IrcCommand.IsStatus(IrcCodes.MessageOfTheDay)))
			{
				// We have a different status, we're in.
				success = true;
			}
		};

		// todo: produce some sort of login animation that doesn't block the thread
		while (!success && sw.ElapsedMilliseconds < TimeoutMs)
		{
			if (rejected)
			{
				_logger.Trace("Irc login rejected: bad credentials");
				break;
			}
			
			// Continue trying
		}

		Client.RawMessageReceived -= LoginCheck;
		_logger.Debug($"Irc login success? {success}");
		return success;
	}
}