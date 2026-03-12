using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace WebApi.Tests;

internal class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Context.Request.Headers.TryGetValue("Test-Authorization", out var userId) || string.IsNullOrEmpty(userId.FirstOrDefault()))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId.First()!),
        new Claim(ClaimTypes.Name, "Test User")
        };
        var identity = new ClaimsIdentity(claims, "TestSchema");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestSchema");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
