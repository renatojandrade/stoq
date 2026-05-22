using Stoq.Inventory.Application.Common.DTOs;

namespace Stoq.Inventory.Application.Ports;

public interface IPhotoReceiptParserService
{
    Task<ParseReceiptResultDto> ParseAsync(
        Stream imageStream,
        string contentType,
        Guid householdId,
        CancellationToken ct = default);
}
