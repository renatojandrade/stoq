using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.RegisterUser;
public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string DisplayName,
    string PreferredLanguage = "en") : IRequest<Result<AuthResponse>>;
