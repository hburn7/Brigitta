using BanchoSharp;

namespace Brigitta.Models;

public class CredentialsModel : IrcCredentials
{
	public bool RememberMe { get; set; }
}