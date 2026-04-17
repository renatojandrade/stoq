namespace Stoq.Identity.Application.Common.DTOs;

/// <summary>Full household details including members, returned to the household admin/member.</summary>
public sealed record HouseholdDetailDto(
    Guid Id,
    string Name,
    string InviteCode,
    DateTimeOffset InviteCodeExpiresAt,
    bool IsInviteCodeExpired,
    IReadOnlyList<MemberDto> Members,
    string Country);

/// <summary>Represents a household member with user profile info.</summary>
public sealed record MemberDto(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    string Role,
    DateTimeOffset JoinedAt,
    bool IsCurrentUser);
