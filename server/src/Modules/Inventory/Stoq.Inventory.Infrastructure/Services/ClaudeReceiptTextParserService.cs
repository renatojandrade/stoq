using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Inventory.Application.Ports;

namespace Stoq.Inventory.Infrastructure.Services;

public sealed partial class ClaudeReceiptTextParserService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<ClaudeReceiptTextParserService> logger)
    : IReceiptTextParserService
{
    private const string _systemPrompt = """
        You are a Brazilian fiscal note (NF-e/NFC-e) parser. Extract information from this consumer page text.

        Return ONLY a valid JSON object (no markdown, no explanations) with exactly these fields:
        - "retailerName": string or null – Razão Social (legal name) of the store
        - "retailerCnpj": string or null – CNPJ digits only, no formatting (14 digits)
        - "retailerTradeName": string or null – Nome Fantasia if shown and different from Razão Social
        - "retailerCity": string or null – city name (Município)
        - "retailerState": string or null – state abbreviation, e.g. SP, RJ, MG
        - "retailerZipCode": string or null – CEP digits only, no dash (8 digits)
        - "retailerStreet": string or null – street name and number (Logradouro + Número)
        - "items": array of objects, each with:
          - "name": string – clean product name in sentence case
          - "quantity": number – quantity purchased (default 1 if not found)
          - "unit": one of: Units, Grams, Kilograms, Millilitres, Litres, Milligrams, Pieces, Boxes, Packs, Bottles, Cans, Bags, Dozens
            Map: UN/UND/PC → Units, KG → Kilograms, G/GR → Grams, L/LIT/LITRO → Litres,
            ML → Millilitres, PCT → Packs, CX → Boxes, FD/SAC → Bags, DZ → Dozens, LT/LATA → Cans
          - "barcode": string or null – EAN barcode if present

        Include only actual product line items. Exclude taxes, totals, fees, discounts, and store metadata.
        """;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [GeneratedRegex(@"\{[\s\S]*\}")]
    private static partial Regex JsonObjectPattern();

    [GeneratedRegex(@"<(script|style)[^>]*>[\s\S]*?</(script|style)>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptStylePattern();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex HtmlTagPattern();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex WhitespacePattern();

    public static string StripHtml(string html)
    {
        var text = ScriptStylePattern().Replace(html, " ");
        text = HtmlTagPattern().Replace(text, " ");
        text = text
            .Replace("&nbsp;", " ", StringComparison.OrdinalIgnoreCase)
            .Replace("&amp;", "&", StringComparison.OrdinalIgnoreCase)
            .Replace("&lt;", "<", StringComparison.OrdinalIgnoreCase)
            .Replace("&gt;", ">", StringComparison.OrdinalIgnoreCase)
            .Replace("&quot;", "\"", StringComparison.OrdinalIgnoreCase)
            .Replace("&#39;", "'", StringComparison.OrdinalIgnoreCase);

        return WhitespacePattern().Replace(text, " ").Trim();
    }

    public async Task<ParseReceiptResultDto> ParseAsync(string text, CancellationToken ct = default)
    {
        var apiKey = configuration["Anthropic:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Anthropic:ApiKey is not configured.");
        }

        var requestBody = new
        {
            model = "claude-haiku-4-5-20251001",
            max_tokens = 1024,
            system = _systemPrompt,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = text,
                },
            },
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = JsonContent.Create(requestBody);

        using HttpResponseMessage response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        AnthropicResponse? anthropicResponse = await response.Content
            .ReadFromJsonAsync<AnthropicResponse>(_jsonOptions, ct);

        if (string.Equals(anthropicResponse?.StopReason, "max_tokens", StringComparison.Ordinal))
        {
            logger.LogWarning("Claude (text parser) hit max_tokens — response was truncated. Increase max_tokens.");
        }

        var responseText = anthropicResponse?.Content?.FirstOrDefault()?.Text ?? string.Empty;
        logger.LogDebug("Claude text parser raw response: {Text}", responseText);
        ParseReceiptResultDto result = ParseJsonResult(responseText);

        if (result.Items.Count == 0)
        {
            logger.LogWarning("Claude text parser returned zero items. Input length: {Len} chars. Raw response: {Text}", text.Length, responseText);
        }

        return result;
    }

    private ParseReceiptResultDto ParseJsonResult(string text)
    {
        var json = JsonObjectPattern().Match(text).Value;
        if (string.IsNullOrWhiteSpace(json))
        {
            json = text.Trim();
        }

        try
        {
            ClaudeReceiptResult? result = JsonSerializer.Deserialize<ClaudeReceiptResult>(json, _jsonOptions);
            if (result is null)
            {
                return new ParseReceiptResultDto(Guid.Empty, null, null, []);
            }

            IReadOnlyList<ParsedReceiptItemDto> items = (result.Items ?? [])
                .Where(i => !string.IsNullOrWhiteSpace(i.Name))
                .Select(i => new ParsedReceiptItemDto(
                    i.Name!,
                    i.Quantity > 0 ? i.Quantity : 1,
                    i.Unit ?? "Units",
                    i.Barcode))
                .ToList();

            var retailerName = string.IsNullOrWhiteSpace(result.RetailerName) ? null : result.RetailerName.Trim();

            RetailerInfoDto? retailerInfo = null;
            if (result.RetailerCnpj is not null
                || result.RetailerTradeName is not null
                || result.RetailerCity is not null
                || result.RetailerState is not null)
            {
                retailerInfo = new RetailerInfoDto(
                    Cnpj: NormaliseDigits(result.RetailerCnpj),
                    LegalName: retailerName,
                    TradeName: string.IsNullOrWhiteSpace(result.RetailerTradeName) ? null : result.RetailerTradeName.Trim(),
                    Street: string.IsNullOrWhiteSpace(result.RetailerStreet) ? null : result.RetailerStreet.Trim(),
                    City: string.IsNullOrWhiteSpace(result.RetailerCity) ? null : result.RetailerCity.Trim(),
                    State: string.IsNullOrWhiteSpace(result.RetailerState) ? null : result.RetailerState.Trim().ToUpperInvariant(),
                    ZipCode: NormaliseDigits(result.RetailerZipCode));
            }

            return new ParseReceiptResultDto(Guid.Empty, retailerName, retailerInfo, items);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse Claude text receipt response as JSON. Raw: {Text}", text);
            return new ParseReceiptResultDto(Guid.Empty, null, null, []);
        }
    }

    private static string? NormaliseDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? null : digits;
    }

    private sealed record AnthropicResponse(IReadOnlyList<ContentBlock>? Content, string? StopReason);

    private sealed record ContentBlock(string? Type, string? Text);

    private sealed record ClaudeReceiptResult(
        string? RetailerName,
        string? RetailerCnpj,
        string? RetailerTradeName,
        string? RetailerStreet,
        string? RetailerCity,
        string? RetailerState,
        string? RetailerZipCode,
        IReadOnlyList<ClaudeItem>? Items);

    private sealed record ClaudeItem(string? Name, decimal Quantity, string? Unit, string? Barcode);
}
