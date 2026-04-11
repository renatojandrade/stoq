using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Stoq.Shared.Kernel.Services;

namespace Stoq.ApiService.Services;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?.User
                .FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(sub, out Guid id))
            {
                return id;
            }

            return null;
        }
    }

    public Guid? HouseholdId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirstValue("household_id");

            if (Guid.TryParse(claim, out Guid id))
            {
                return id;
            }

            return null;
        }
    }

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
