using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ReferenceDataApi.Security;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IOptionsMonitor<ApiKeyOptions> _apiKeyOptions;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> schemeOptions,
        IOptionsMonitor<ApiKeyOptions> apiKeyOptions,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(schemeOptions, logger, encoder)
    {
        _apiKeyOptions = apiKeyOptions;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var cfg = _apiKeyOptions.CurrentValue;
        var headerName = string.IsNullOrWhiteSpace(cfg.HeaderName) ? "X-API-KEY" : cfg.HeaderName;

        if (!Request.Headers.TryGetValue(headerName, out var values))
            return Task.FromResult(AuthenticateResult.Fail($"Missing API key header '{headerName}'"));

        var providedKey = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(providedKey))
            return Task.FromResult(AuthenticateResult.Fail("API key was empty"));

        var match = cfg.Keys.FirstOrDefault(k => string.Equals(k.Key, providedKey, StringComparison.Ordinal));
        if (match is null)
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "ApiKeyUser"),
            new(ClaimTypes.Name, "ApiKeyUser"),
            new(ClaimTypes.Role, match.Role)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}