namespace Stoq.Inventory.Application.Common.DTOs;

public sealed record RetailerInfoDto(
    string? Cnpj,
    string? LegalName,
    string? TradeName,
    string? Street,
    string? City,
    string? State,
    string? ZipCode);
