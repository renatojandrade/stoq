using MediatR;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.Logout;
public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
