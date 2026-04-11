using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.RefreshToken;
public sealed record RefreshTokenCommand(
    string ExpiredAccessToken,
    string RefreshToken) : IRequest<Result<AuthResponse>>;
