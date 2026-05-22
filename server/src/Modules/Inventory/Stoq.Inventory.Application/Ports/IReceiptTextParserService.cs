using Stoq.Inventory.Application.Common.DTOs;

namespace Stoq.Inventory.Application.Ports;

public interface IReceiptTextParserService
{
    Task<ParseReceiptResultDto> ParseAsync(string text, CancellationToken ct = default);
}
