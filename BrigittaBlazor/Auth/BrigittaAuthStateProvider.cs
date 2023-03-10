using BanchoSharp.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace BrigittaBlazor.Auth;

public class BrigittaAuthStateProvider : AuthenticationStateProvider
{
	private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
	private readonly IBanchoClient _client;
	private readonly NavigationManager _navManager;
	private readonly ProtectedSessionStorage _sessionStorage;

	public BrigittaAuthStateProvider(ProtectedSessionStorage sessionStorage, IBanchoClient client, NavigationManager navManager)
	{
		_sessionStorage = sessionStorage;
		_client = client;
		_navManager = navManager;
	}

	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		try
		{
			var userSessionStorageResult = await _sessionStorage.GetAsync<IBanchoClient>("UserSession");
			var userSessionClient = userSessionStorageResult.Success ? userSessionStorageResult.Value : null;
			if (userSessionClient == null)
			{
				return await Task.FromResult(new AuthenticationState(_anonymous));
			}

			var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
			{
				new(ClaimTypes.Name, userSessionClient.ClientConfig.Credentials.Username)
			}, "BrigittaAuth"));

			return await Task.FromResult(new AuthenticationState(claimsPrincipal));
		}
		catch
		{
			return await Task.FromResult(new AuthenticationState(_anonymous));
		}
	}

	public async Task UpdateAuthenticationStateAsync(IBanchoClient? client)
	{
		ClaimsPrincipal claimsPrincipal;

		if (client is { IsAuthenticated: true })
		{
			// User is logged in and authenticated
			await _sessionStorage.SetAsync("UserSession", client);
			claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
			{
				new(ClaimTypes.Name, client.ClientConfig.Credentials.Username)
			}, "BrigittaAuth"));
		}
		else
		{
			await _sessionStorage.DeleteAsync("UserSession");
			claimsPrincipal = _anonymous;
		}

		NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
		_navManager.NavigateTo("/primarydisplay");
	}
}