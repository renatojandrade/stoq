namespace Stoq.Identity.Application.Common.DTOs;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    UserDto User);

public sealed record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string PreferredLanguage,
    string Role,
    Guid? HouseholdId);

public sealed record HouseholdDto(
    Guid Id,
    string Name,
    string InviteCode,
    int MemberCount);
