using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;

namespace Stoq.Catalog.Infrastructure.Services;

internal sealed class OpenFoodFactsService(HttpClient httpClient) : IOpenFoodFactsService
{
    private static readonly Dictionary<string, string> _countryTags = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BR"] = "brazil",
        ["US"] = "united-states",
    };

    public async Task<ProductLookupDto?> LookupByBarcodeAsync(string barcode, CancellationToken ct = default)
    {
        OpenFoodFactsResponse? response = await httpClient
            .GetFromJsonAsync<OpenFoodFactsResponse>($"api/v0/product/{barcode}.json", ct);

        if (response is null || response.Status != 1 || response.Product is null)
        {
            return null;
        }

        return MapProduct(barcode, response.Product);
    }

    public async Task<IReadOnlyList<ProductLookupDto>> SearchByNameAsync(string name, string? country, CancellationToken ct = default)
    {
        var url = BuildSearchUrl(name, country);

        OpenFoodFactsSearchResponse? response = await httpClient
            .GetFromJsonAsync<OpenFoodFactsSearchResponse>(url, ct);

        if (response?.Products is null || response.Products.Length == 0)
        {
            return [];
        }

        var results = new List<ProductLookupDto>();

        foreach (OpenFoodFactsProduct product in response.Products)
        {
            var name2 = product.ProductName ?? product.ProductNameEn;
            if (string.IsNullOrWhiteSpace(name2))
            {
                continue;
            }

            var barcode = string.IsNullOrWhiteSpace(product.Code) ? null : product.Code;
            var brand = string.IsNullOrWhiteSpace(product.Brands) ? null : product.Brands.Split(',')[0].Trim();
            var category = ParseCategory(product.CategoriesHierarchy, product.Categories);

            results.Add(new ProductLookupDto(
                barcode ?? string.Empty,
                name2.Trim(),
                brand,
                category,
                product.ImageFrontUrl ?? product.ImageUrl));
        }

        return results;
    }

    private static string BuildSearchUrl(string name, string? country)
    {
        var encoded = Uri.EscapeDataString(name);
        var url = $"cgi/search.pl?search_terms={encoded}&json=1&page_size=10&action=process";

        if (country is not null && _countryTags.TryGetValue(country, out var tag))
        {
            url += $"&countries_tags_en={tag}";
        }

        return url;
    }

    private static ProductLookupDto? MapProduct(string barcode, OpenFoodFactsProduct product)
    {
        var name = product.ProductName ?? product.ProductNameEn;
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var brand = string.IsNullOrWhiteSpace(product.Brands) ? null : product.Brands.Split(',')[0].Trim();
        var category = ParseCategory(product.CategoriesHierarchy, product.Categories);

        return new ProductLookupDto(
            barcode,
            name.Trim(),
            brand,
            category,
            product.ImageFrontUrl ?? product.ImageUrl);
    }

    private static string? ParseCategory(string[]? hierarchy, string? rawCategories)
    {
        // Prefer the most-specific English tag from the hierarchy, e.g. "en:yogurts" → "Yogurts"
        if (hierarchy is { Length: > 0 })
        {
            for (var i = hierarchy.Length - 1; i >= 0; i--)
            {
                var tag = hierarchy[i];
                if (tag.StartsWith("en:", StringComparison.OrdinalIgnoreCase))
                {
                    var label = tag["en:".Length..].Replace('-', ' ');
                    return char.ToUpperInvariant(label[0]) + label[1..];
                }
            }
        }

        // Fall back to the first item in the comma-separated categories string.
        if (!string.IsNullOrWhiteSpace(rawCategories))
        {
            return rawCategories.Split(',')[0].Trim();
        }

        return null;
    }

    private sealed class OpenFoodFactsResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("product")]
        public OpenFoodFactsProduct? Product { get; set; }
    }

    private sealed class OpenFoodFactsSearchResponse
    {
        [JsonPropertyName("products")]
        public OpenFoodFactsProduct[]? Products { get; set; }
    }

    private sealed class OpenFoodFactsProduct
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("product_name")]
        public string? ProductName { get; set; }

        [JsonPropertyName("product_name_en")]
        public string? ProductNameEn { get; set; }

        [JsonPropertyName("brands")]
        public string? Brands { get; set; }

        [JsonPropertyName("categories")]
        public string? Categories { get; set; }

        [JsonPropertyName("categories_hierarchy")]
        public string[]? CategoriesHierarchy { get; set; }

        [JsonPropertyName("image_front_url")]
        public string? ImageFrontUrl { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }
    }
}
