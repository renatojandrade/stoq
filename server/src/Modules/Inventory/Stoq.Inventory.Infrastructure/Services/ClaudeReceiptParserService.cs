using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Entities;

namespace Stoq.Inventory.Infrastructure.Services;

public sealed partial class ClaudeReceiptParserService(
    HttpClient httpClient,
    IConfiguration configuration,
    IParseCorrectionsRepository correctionsRepository,
    ILogger<ClaudeReceiptParserService> logger)
    : IPhotoReceiptParserService
{
    private const string _baseSystemPrompt = """
        You are a grocery receipt parser. Extract information from this receipt image.

        Return ONLY a valid JSON object (no markdown, no explanations) with exactly these fields:
        - "retailerName": string or null – the name of the store/supermarket as it appears on the receipt
        - "items": array of objects, each with:
          - "name": string – clean product name in sentence case
          - "quantity": number – quantity purchased (default 1 if not shown)
          - "unit": one of: Units, Grams, Kilograms, Millilitres, Litres, Milligrams, Pieces, Boxes, Packs, Bottles, Cans, Bags, Dozens
            Map receipt abbreviations: UN/UND/PC → Units, KG → Kilograms, G/GR → Grams, L/LIT/LITRO → Litres,
            ML → Millilitres, PCT → Packs, CX → Boxes, FD/SAC → Bags, DZ → Dozens, LT/LATA → Cans
          - "barcode": string or null – EAN barcode if visible on receipt

        Do not include taxes, subtotals, fees, discounts, or store metadata in items. Only actual product line items.
        """;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [GeneratedRegex(@"\{[\s\S]*\}")]
    private static partial Regex JsonObjectPattern();

    public async Task<ParseReceiptResultDto> ParseAsync(
        Stream imageStream,
        string contentType,
        Guid householdId,
        CancellationToken ct = default)
    {
        var apiKey = configuration["Anthropic:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Anthropic:ApiKey is not configured.");
        }

        using var ms = new MemoryStream();
        await imageStream.CopyToAsync(ms, ct);
        var base64Image = Convert.ToBase64String(ms.ToArray());

        var systemPrompt = await BuildSystemPromptAsync(householdId, ct);

        var requestBody = new
        {
            model = "claude-sonnet-4-6",
            max_tokens = 1024,
            system = systemPrompt,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "image",
                            source = new
                            {
                                type = "base64",
                                media_type = contentType,
                                data = base64Image,
                            },
                        },
                        new
                        {
                            type = "text",
                            text = "Parse the retailer name and all grocery items from this receipt.",
                        },
                    },
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
            logger.LogWarning("Claude (photo parser) hit max_tokens — response was truncated. Increase max_tokens.");
        }

        var text = anthropicResponse?.Content?.FirstOrDefault()?.Text ?? string.Empty;
        logger.LogDebug("Claude photo parser raw response: {Text}", text);
        ParseReceiptResultDto result = ParseJsonResult(text);

        if (result.Items.Count == 0)
        {
            logger.LogWarning("Claude photo parser returned zero items. Raw response: {Text}", text);
        }

        return result;
    }

    private async Task<string> BuildSystemPromptAsync(Guid householdId, CancellationToken ct)
    {
        IReadOnlyList<ParseCorrectionItem> recentItems = await correctionsRepository
            .GetRecentConfirmedItemsAsync(householdId, retailerName: null, limit: 5, ct);

        if (recentItems.Count == 0)
        {
            return _baseSystemPrompt;
        }

        var examples = new StringBuilder();
        examples.AppendLine();
        examples.AppendLine("Previously confirmed items for this household (use as a reference for product name formatting and unit selection):");

        foreach (ParseCorrectionItem item in recentItems)
        {
            examples.Append("- ").Append(item.ProductName).Append(" → ").AppendLine(item.Unit);
        }

        return _baseSystemPrompt + examples.ToString();
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
            return new ParseReceiptResultDto(Guid.Empty, retailerName, null, items);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse Claude receipt response as JSON. Raw: {Text}", text);
            return new ParseReceiptResultDto(Guid.Empty, null, null, []);
        }
    }

    private sealed record AnthropicResponse(IReadOnlyList<ContentBlock>? Content, string? StopReason);

    private sealed record ContentBlock(string? Type, string? Text);

    private sealed record ClaudeReceiptResult(string? RetailerName, IReadOnlyList<ClaudeItem>? Items);

    private sealed record ClaudeItem(string? Name, decimal Quantity, string? Unit, string? Barcode);
}
