using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Stoq.Identity.Application.Ports;

namespace Stoq.Identity.Infrastructure.Services;

internal sealed class SocialAuthServiceImpl(HttpClient httpClient, IConfiguration configuration)
    : ISocialAuthService
{
#pragma warning disable S1075 // These are well-known public OAuth/OIDC endpoints, not configurable URIs.
    private const string _appleJwksUri = "https://appleid.apple.com/.well-known/openid-configuration";
    private const string _googleTokenInfoUrl = "https://oauth2.googleapis.com/tokeninfo?id_token=";
    private const string _facebookGraphUrl = "https://graph.facebook.com/me?fields=id,email,name&access_token=";
#pragma warning restore S1075

    public async Task<SocialUserInfo?> ValidateGoogleTokenAsync(string idToken, CancellationToken ct = default)
    {
        GoogleTokenInfoResponse? response = await httpClient
            .GetFromJsonAsync<GoogleTokenInfoResponse>(_googleTokenInfoUrl + Uri.EscapeDataString(idToken), ct)
            ;

        if (response is null || string.IsNullOrEmpty(response.Sub))
        {
            return null;
        }

        return new SocialUserInfo(response.Sub, response.Email ?? string.Empty, response.Name, response.Picture);
    }

    public async Task<SocialUserInfo?> ValidateFacebookTokenAsync(string accessToken, CancellationToken ct = default)
    {
        FacebookUserResponse? response = await httpClient
            .GetFromJsonAsync<FacebookUserResponse>(_facebookGraphUrl + Uri.EscapeDataString(accessToken), ct)
            ;

        if (response is null || string.IsNullOrEmpty(response.Id))
        {
            return null;
        }

        return new SocialUserInfo(response.Id, response.Email ?? string.Empty, response.Name, null);
    }

    public async Task<SocialUserInfo?> ValidateAppleTokenAsync(string identityToken, CancellationToken ct = default)
    {
        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            _appleJwksUri,
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever(httpClient));

        OpenIdConnectConfiguration oidcConfig = await configManager.GetConfigurationAsync(ct);

        var clientId = configuration["Identity:AppleClientId"]
            ?? throw new InvalidOperationException("Identity:AppleClientId is not configured.");

        var validationParameters = new TokenValidationParameters
        {
            ValidIssuer = "https://appleid.apple.com",
            ValidAudience = clientId,
            IssuerSigningKeys = oidcConfig.SigningKeys,
            ValidateLifetime = true,
        };

        try
        {
            ClaimsPrincipal principal = new JwtSecurityTokenHandler()
                .ValidateToken(identityToken, validationParameters, out _);

            var sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? principal.FindFirst("sub")?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value
                        ?? principal.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(sub))
            {
                return null;
            }

            return new SocialUserInfo(sub, email ?? string.Empty, null, null);
        }
        catch
        {
            return null;
        }
    }

    // Response DTOs
    private sealed class GoogleTokenInfoResponse
    {
        [JsonPropertyName("sub")]
        public string? Sub { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
    }

    private sealed class FacebookUserResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
