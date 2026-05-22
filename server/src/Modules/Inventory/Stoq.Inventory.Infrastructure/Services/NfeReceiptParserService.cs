using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Inventory.Application.Ports;

namespace Stoq.Inventory.Infrastructure.Services;

public sealed class NfeReceiptParserService(
    HttpClient httpClient,
    IReceiptTextParserService textParser,
    PlaywrightBrowserService pageRenderer,
    ILogger<NfeReceiptParserService> logger)
    : INfeReceiptParserService
{
    private static readonly XNamespace _nfeNs = "http://www.portalfiscal.inf.br/nfe";

    public async Task<ParseReceiptResultDto> ParseAsync(
        string qrCodeUrl,
        CancellationToken ct = default)
    {
        string responseBody;
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(qrCodeUrl, ct);
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to fetch NF-e QR code URL: {Url}", qrCodeUrl);
            throw new InvalidOperationException("Unable to retrieve NF-e data from the provided URL.", ex);
        }

        var trimmed = responseBody.TrimStart();
        var isHtml = !trimmed.StartsWith('<')
            || trimmed.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("<html", StringComparison.OrdinalIgnoreCase);

        if (isHtml)
        {
            logger.LogInformation("NF-e URL returned HTML (JS portal) — using Playwright to render page.");
            var renderedHtml = await pageRenderer.GetRenderedHtmlAsync(qrCodeUrl, ct);
            var pageText = ClaudeReceiptTextParserService.StripHtml(renderedHtml);
            logger.LogInformation(
                "Playwright rendered text preview: {Preview}",
                pageText[..Math.Min(500, pageText.Length)]);
            return await textParser.ParseAsync(pageText, ct);
        }

        return ParseNfeXml(responseBody);
    }

    private ParseReceiptResultDto ParseNfeXml(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);

            XElement? ide = doc.Descendants(_nfeNs + "ide").FirstOrDefault();
            var receiptNumber = Trim(ide?.Element(_nfeNs + "nNF")?.Value);
            var emittedAtRaw = Trim(ide?.Element(_nfeNs + "dhEmi")?.Value);
            DateTimeOffset? emittedAt = DateTimeOffset.TryParse(emittedAtRaw, out var parsed) ? parsed : null;

            XElement? emit = doc.Descendants(_nfeNs + "emit").FirstOrDefault();
            XElement? enderEmit = emit?.Element(_nfeNs + "enderEmit");

            var retailerName = Trim(emit?.Element(_nfeNs + "xNome")?.Value);
            var cnpj = NormaliseDigits(emit?.Element(_nfeNs + "CNPJ")?.Value);
            var tradeName = Trim(emit?.Element(_nfeNs + "xFant")?.Value);

            var street = Trim(emit?.Element(_nfeNs + "xLgr")?.Value ?? enderEmit?.Element(_nfeNs + "xLgr")?.Value);
            var number = Trim(emit?.Element(_nfeNs + "nro")?.Value ?? enderEmit?.Element(_nfeNs + "nro")?.Value);
            var fullStreet = street is not null && number is not null ? $"{street}, {number}" : street ?? number;

            var city = Trim(emit?.Element(_nfeNs + "xMun")?.Value ?? enderEmit?.Element(_nfeNs + "xMun")?.Value);
            var state = Trim(emit?.Element(_nfeNs + "UF")?.Value ?? enderEmit?.Element(_nfeNs + "UF")?.Value)?.ToUpperInvariant();
            var zipCode = NormaliseDigits(emit?.Element(_nfeNs + "CEP")?.Value ?? enderEmit?.Element(_nfeNs + "CEP")?.Value);

            RetailerInfoDto? retailerInfo = null;
            if (cnpj is not null || tradeName is not null || city is not null || state is not null)
            {
                retailerInfo = new RetailerInfoDto(
                    Cnpj: cnpj,
                    LegalName: retailerName,
                    TradeName: tradeName,
                    Street: fullStreet,
                    City: city,
                    State: state,
                    ZipCode: zipCode);
            }

            IEnumerable<XElement> detElements = doc.Descendants(_nfeNs + "det");
            var items = new List<ParsedReceiptItemDto>();

            foreach (XElement det in detElements)
            {
                XElement? prod = det.Element(_nfeNs + "prod");
                if (prod is null)
                {
                    continue;
                }

                var name = prod.Element(_nfeNs + "xProd")?.Value;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var item = ParseNfeItem(_nfeNs, prod, name);
                items.Add(item);
            }

            return new ParseReceiptResultDto(Guid.Empty, retailerName, retailerInfo, items, receiptNumber, emittedAt);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to parse NF-e XML.");
            throw new InvalidOperationException("Failed to parse the NF-e XML structure.", ex);
        }
    }

    private static ParsedReceiptItemDto ParseNfeItem(XNamespace ns, XElement prod, string name)
    {
        var quantityStr = prod.Element(ns + "qCom")?.Value;
        var unitStr = prod.Element(ns + "uCom")?.Value;
        var barcode = prod.Element(ns + "cEAN")?.Value;
        var retailerSku = Trim(prod.Element(ns + "cProd")?.Value);

        var quantity = decimal.TryParse(
                quantityStr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var q)
            ? q
            : 1m;

        var unitPrice = ParseDecimal(prod.Element(ns + "vUnCom")?.Value);
        var totalPrice = ParseDecimal(prod.Element(ns + "vProd")?.Value);
        var rawDiscount = ParseDecimal(prod.Element(ns + "vDesc")?.Value);
        var discount = rawDiscount > 0 ? rawDiscount : null;

        var unit = MapUnit(unitStr);
        var normalizedBarcode = string.IsNullOrWhiteSpace(barcode) || barcode == "SEM GTIN" ? null : barcode;

        return new ParsedReceiptItemDto(
            name,
            quantity,
            unit,
            normalizedBarcode,
            RetailerSku: retailerSku,
            UnitPrice: unitPrice,
            TotalPrice: totalPrice,
            Discount: discount);
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.TryParse(
            value,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var result)
            ? result
            : null;
    }

    private static string? Trim(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormaliseDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? null : digits;
    }

    private static string MapUnit(string? rawUnit) =>
        rawUnit?.ToUpperInvariant() switch
        {
            "KG" or "KGS" => "Kilograms",
            "G" or "GR" or "GRS" or "GRAMA" or "GRAMAS" => "Grams",
            "MG" => "Milligrams",
            "L" or "LIT" or "LITRO" or "LITROS" => "Litres",
            "ML" => "Millilitres",
            "PCT" or "PACOTE" or "PACOTES" => "Packs",
            "CX" or "CAIXA" or "CAIXAS" => "Boxes",
            "FD" or "SAC" or "SACOLA" => "Bags",
            "DZ" or "DUZIA" or "DUZIAS" => "Dozens",
            "GRF" or "GARRAFA" => "Bottles",
            "LT" or "LATA" => "Cans",
            _ => "Units",
        };
}
