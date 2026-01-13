using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace SportsCenter.API.Extensions.Auth;

public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var headerValues))
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header"));

        var authHeader = headerValues.ToString();
        if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.Fail("Invalid scheme"));

        var encoded = authHeader["Basic ".Length..].Trim();

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Base64"));
        }

        var parts = decoded.Split(':', 2);
        if (parts.Length != 2)
            return Task.FromResult(AuthenticateResult.Fail("Invalid credentials format"));

        var username = parts[0];
        var password = parts[1];

        var (ok, roles) = Validate(username, password);
        if (!ok)
            return Task.FromResult(AuthenticateResult.Fail("Bad credentials"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }

    
    private static (bool ok, string[] roles) Validate(string u, string p)
    {
        if (u == "user" && p == "123") return (true, new[] { "USER" });
        if (u == "admin" && p == "123") return (true, new[] { "ADMIN" });
        return (false, Array.Empty<string>());
    }
}