using MediatR;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Identity.Application.Auth.Commands.ResetPassword;
public sealed record ResetPasswordCommand(
    string Email,
    string OtpCode,
    string NewPassword) : IRequest<Result>;
