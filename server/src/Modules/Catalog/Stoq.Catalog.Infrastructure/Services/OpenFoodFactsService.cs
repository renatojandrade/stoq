using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;

namespace Stoq.Catalog.Infrastructure.Services;

internal sealed class OpenFoodFactsService(HttpClient httpClient) : IOpenFoodFactsService
{
    public async Task<ProductLookupDto?> LookupByBarcodeAsync(string barcode, CancellationToken ct = default)
    {
        OpenFoodFactsResponse? response = await httpClient
            .GetFromJsonAsync<OpenFoodFactsResponse>($"api/v0/product/{barcode}.json", ct)
            .ConfigureAwait(false);

        if (response is null || response.Status != 1 || response.Product is null)
        {
            return null;
        }

        OpenFoodFactsProduct product = response.Product;

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
            for (int i = hierarchy.Length - 1; i >= 0; i--)
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

    private sealed class OpenFoodFactsProduct
    {
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
