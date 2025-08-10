using System.Net.Http.Json;

namespace BourbonVault.API.Services
{
    public interface IUpcLookupService
    {
        Task<UpcLookupResult?> LookupAsync(string barcode, CancellationToken ct = default);
    }

    public class UpcLookupSettings
    {
        public string? Provider { get; set; } // "UPCItemDB" or "OpenFoodFacts"
        public string? ApiKey { get; set; }
        public string? ApiBaseUrl { get; set; }
    }

    public class UpcLookupService : IUpcLookupService
    {
        private readonly HttpClient _http;
        private readonly UpcLookupSettings _settings;
        public UpcLookupService(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _settings = cfg.GetSection("UpcLookup").Get<UpcLookupSettings>() ?? new UpcLookupSettings();
        }

        public async Task<UpcLookupResult?> LookupAsync(string barcode, CancellationToken ct = default)
        {
            var provider = (_settings.Provider ?? "").ToLowerInvariant();
            if (provider == "upcitemdb")
            {
                return await LookupWithUpcItemDb(barcode, ct);
            }
            else if (provider == "openfoodfacts")
            {
                return await LookupWithOpenFoodFacts(barcode, ct);
            }
            return null;
        }

        private async Task<UpcLookupResult?> LookupWithUpcItemDb(string barcode, CancellationToken ct)
        {
            // https://www.upcitemdb.com/api/explorer
            // GET https://api.upcitemdb.com/prod/trial/lookup?upc={barcode}
            var baseUrl = _settings.ApiBaseUrl?.TrimEnd('/') ?? "https://api.upcitemdb.com";
            var path = baseUrl.Contains("/prod/") ? baseUrl : baseUrl + "/prod/trial";
            var url = $"{path}/lookup?upc={Uri.EscapeDataString(barcode)}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                req.Headers.Add("Authorization", _settings.ApiKey);
            }
            using var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return null;
            var payload = await resp.Content.ReadFromJsonAsync<UpcItemDbPayload>(cancellationToken: ct);
            var item = payload?.items?.FirstOrDefault();
            if (item == null) return null;

            return new UpcLookupResult
            {
                Name = item.title,
                Brand = item.brand,
                ImageUrl = item.images?.FirstOrDefault(),
                // Map limited fields; proof/age/volume often absent
                Type = InferTypeFromTitle(item.title) ?? item.category,
            };
        }

        private async Task<UpcLookupResult?> LookupWithOpenFoodFacts(string barcode, CancellationToken ct)
        {
            // https://world.openfoodfacts.org/api/v2/product/{barcode}
            var baseUrl = _settings.ApiBaseUrl?.TrimEnd('/') ?? "https://world.openfoodfacts.org";
            var url = $"{baseUrl}/api/v2/product/{Uri.EscapeDataString(barcode)}";
            var payload = await _http.GetFromJsonAsync<OpenFoodFactsPayload>(url, ct);
            var p = payload?.product;
            if (p == null) return null;
            return new UpcLookupResult
            {
                Name = p.product_name ?? p.generic_name,
                Brand = (p.brands_tags != null && p.brands_tags.Length > 0) ? p.brands_tags[0] : p.brands,
                ImageUrl = p.image_url,
                Type = InferTypeFromTitle(p.product_name) ?? "Bourbon",
            };
        }

        private static string? InferTypeFromTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) return null;
            var t = title.ToLowerInvariant();
            if (t.Contains("rye")) return "Rye";
            if (t.Contains("scotch")) return "Scotch";
            if (t.Contains("bourbon")) return "Bourbon";
            return null;
        }

        private class UpcItemDbPayload
        {
            public UpcItemDbItem[]? items { get; set; }
        }
        private class UpcItemDbItem
        {
            public string? title { get; set; }
            public string? brand { get; set; }
            public string? category { get; set; }
            public List<string>? images { get; set; }
        }

        private class OpenFoodFactsPayload
        {
            public OpenFoodFactsProduct? product { get; set; }
        }
        private class OpenFoodFactsProduct
        {
            public string? product_name { get; set; }
            public string? generic_name { get; set; }
            public string? brands { get; set; }
            public string[]? brands_tags { get; set; }
            public string? image_url { get; set; }
        }
    }

    public class UpcLookupResult
    {
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public string? ImageUrl { get; set; }
        public string? Type { get; set; }
        public decimal? Proof { get; set; }
        public int? VolumeMl { get; set; }
        public int? AgeYears { get; set; }
    }
}
