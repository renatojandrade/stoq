using MediatR;
using Stoq.Identity.Application.Common.DTOs;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.LoginUser;
public sealed record LoginUserCommand(
    string Email,
    string Password) : IRequest<Result<AuthResponse>>;
