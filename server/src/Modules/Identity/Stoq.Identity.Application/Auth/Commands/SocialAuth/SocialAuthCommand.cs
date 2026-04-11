using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.SocialAuth;

public enum SocialProvider
{
    Google,
    Facebook,
    Apple,
}

public sealed record SocialAuthCommand(
    SocialProvider Provider,
    string ProviderToken,
    string? DisplayName = null) : IRequest<Result<AuthResponse>>;
