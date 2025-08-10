using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BourbonVault.Web.Services
{
    public interface IVisionService
    {
        Task<VisionLookupResult> LookupAsync(IBrowserFile file, CancellationToken ct = default);
    }

    public class VisionService : IVisionService
    {
        private readonly HttpClient _http;
        public VisionService(HttpClient http) { _http = http; }

        public async Task<VisionLookupResult> LookupAsync(IBrowserFile file, CancellationToken ct = default)
        {
            var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024, cancellationToken: ct);
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            content.Add(streamContent, name: "file", fileName: file.Name);

            using var response = await _http.PostAsync("api/vision/lookup", content, ct);
            var payload = await response.Content.ReadFromJsonAsync<VisionLookupPayload>(cancellationToken: ct);
            if (payload == null)
                return new VisionLookupResult(false, null, null, null);
            return new VisionLookupResult(payload.success, payload.barcode, payload.format, payload.suggested);
        }
    }

    public record VisionLookupResult(bool Success, string? Barcode, string? Format, SuggestedBottle? Suggested);

    public class SuggestedBottle
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Brand { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Proof { get; set; }
        public int? VolumeMl { get; set; }
        public int? AgeYears { get; set; }
    }

    internal class VisionLookupPayload
    {
        public bool success { get; set; }
        public string? barcode { get; set; }
        public string? format { get; set; }
        public SuggestedBottle? suggested { get; set; }
    }
}
