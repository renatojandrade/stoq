using Stoq.Inventory.Application.Common.DTOs;

namespace Stoq.Inventory.Application.Ports;

public interface INfeReceiptParserService
{
    Task<ParseReceiptResultDto> ParseAsync(
        string qrCodeUrl,
        CancellationToken ct = default);
}
