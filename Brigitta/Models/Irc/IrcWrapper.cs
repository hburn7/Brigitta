using BanchoSharp;
using NLog;
using System.Diagnostics;

namespace Brigitta.Models.Irc;

public class IrcWrapper
{
	private static readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

	public IrcWrapper(Credentials credentials)
	{
		Credentials = credentials;
		Client = new BanchoClient(new BanchoClientConfig(
			new Credentials(credentials.Username, credentials.Password, credentials.RememberMe), BanchoSharp.LogLevel.Debug));
	}

	public IrcWrapper() : this(new Credentials(string.Empty, string.Empty, false).Load()) {}
	public BanchoClient Client { get; }
	public Credentials Credentials { get; }
}