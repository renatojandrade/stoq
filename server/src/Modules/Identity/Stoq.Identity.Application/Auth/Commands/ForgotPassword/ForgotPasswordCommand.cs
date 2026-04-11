using MediatR;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.ForgotPassword;
public sealed record ForgotPasswordCommand(string Email) : IRequest<Result>;
