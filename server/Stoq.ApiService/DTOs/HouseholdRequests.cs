namespace Stoq.ApiService.DTOs;

/// <summary>Request body for creating a household.</summary>
public sealed record CreateHouseholdRequest(string Name);

/// <summary>Request body for joining a household via invite code.</summary>
public sealed record JoinHouseholdRequest(string InviteCode);

/// <summary>Request body for changing a member's role.</summary>
public sealed record ChangeMemberRoleRequest(string Role);
