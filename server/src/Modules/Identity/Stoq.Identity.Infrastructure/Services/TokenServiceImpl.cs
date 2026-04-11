using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Domain.Enums;

namespace Stoq.Identity.Infrastructure.Services;

internal sealed class TokenServiceImpl(IConfiguration configuration)
    : ITokenService
{
    private const int _accessTokenLifetimeMinutes = 15;

    public string GenerateAccessToken(Guid userId, string email, AppRole role, Guid? householdId)
    {
        SymmetricSecurityKey key = GetSigningKey();
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("role", role.ToString()),
        };

        if (householdId.HasValue)
        {
            claims.Add(new Claim("household_id", householdId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: configuration["Identity:JwtIssuer"],
            audience: configuration["Identity:JwtAudience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenLifetimeMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    public Guid? GetUserIdFromExpiredToken(string accessToken)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GetSigningKey(),
            ValidateLifetime = false,
        };

        try
        {
            ClaimsPrincipal principal = new JwtSecurityTokenHandler()
                .ValidateToken(accessToken, validationParameters, out _);

            var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (Guid.TryParse(sub, out Guid userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private SymmetricSecurityKey GetSigningKey()
    {
        var secret = configuration["Identity:JwtSigningKey"]
            ?? throw new InvalidOperationException("Identity:JwtSigningKey is not configured.");
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }
}
