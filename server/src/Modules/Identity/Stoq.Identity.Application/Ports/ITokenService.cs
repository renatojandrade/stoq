using Stoq.Identity.Domain.Enums;

namespace Stoq.Identity.Application.Ports;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, AppRole role, Guid? householdId, HouseholdCountry? householdCountry = null);
    string GenerateRefreshToken();
    Guid? GetUserIdFromExpiredToken(string accessToken);
}
