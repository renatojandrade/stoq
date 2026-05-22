using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Stoq.Catalog.Application.Common.DTOs;
using Stoq.Catalog.Application.Ports;

namespace Stoq.Catalog.Infrastructure.Services;

internal sealed class CosmosProductService(HttpClient httpClient) : ICosmosProductService
{
    public async Task<ProductLookupDto?> LookupByBarcodeAsync(string barcode, CancellationToken ct = default)
    {
        HttpResponseMessage response = await httpClient
            .GetAsync($"gtins/{barcode}.json", ct);

        if (response.StatusCode == HttpStatusCode.NotFound
            || response.StatusCode == HttpStatusCode.Unauthorized
            || !response.IsSuccessStatusCode)
        {
            return null;
        }

        CosmosProductResponse? product = await response.Content
            .ReadFromJsonAsync<CosmosProductResponse>(cancellationToken: ct);

        if (product is null || string.IsNullOrWhiteSpace(product.Description))
        {
            return null;
        }

        return new ProductLookupDto(
            barcode,
            product.Description.Trim(),
            string.IsNullOrWhiteSpace(product.Brand?.Name) ? null : product.Brand.Name.Trim(),
            string.IsNullOrWhiteSpace(product.CategoryName) ? null : product.CategoryName.Trim(),
            product.Thumbnail ?? product.Image);
    }

    private sealed class CosmosProductResponse
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("category_name")]
        public string? CategoryName { get; set; }

        [JsonPropertyName("brand")]
        public CosmosBrand? Brand { get; set; }
    }

    private sealed class CosmosBrand
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
