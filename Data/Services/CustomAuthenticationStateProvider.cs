using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace it_tools.Data.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSession = await _sessionStorage.GetAsync<string>("userEmail");
            if (string.IsNullOrEmpty(userSession.Value))
            {
                return new AuthenticationState(_anonymous);
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userSession.Value),
                new Claim(ClaimTypes.Email, userSession.Value),
                new Claim(ClaimTypes.NameIdentifier, userSession.Value)
            };

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task NotifyUserAuthentication(string email)
    {
        await _sessionStorage.SetAsync("userEmail", email);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Email, email),
        };

        var identity = new ClaimsIdentity(claims, "CustomAuth");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task NotifyUserLogout()
    {
        await _sessionStorage.DeleteAsync("userEmail");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}