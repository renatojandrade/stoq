using MediatR;
using Stoq.Shared.Kernel.Common;

namespace Stoq.Inventory.Application.Receipts.Commands.ParseNfe;

public sealed record ParseNfeCommand(
    string QrCodeUrl,
    Guid HouseholdId,
    Guid InitiatedByUserId,
    int MaxPendingJobsPerUser) : IRequest<Result<Guid>>;
