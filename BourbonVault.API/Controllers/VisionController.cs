using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using System.Drawing;
using BourbonVault.API.Services;

namespace BourbonVault.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisionController : ControllerBase
    {
        private readonly IUpcLookupService _upc;
        public VisionController(IUpcLookupService upc) { _upc = upc; }
        [AllowAnonymous]
        [HttpPost("lookup")]
        [RequestSizeLimit(10_000_000)] // ~10 MB
        public async Task<IActionResult> Lookup([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "No image provided" });

            try
            {
                await using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;

                using var bmp = new Bitmap(ms);
                var reader = new BarcodeReaderGeneric
                {
                    AutoRotate = true,
                    Options = new DecodingOptions
                    {
                        TryHarder = true,
                        PossibleFormats = new List<BarcodeFormat>
                        {
                            BarcodeFormat.EAN_13,
                            BarcodeFormat.EAN_8,
                            BarcodeFormat.UPC_A,
                            BarcodeFormat.UPC_E,
                            BarcodeFormat.CODE_128,
                            BarcodeFormat.CODE_39,
                        }
                    }
                };

                // Convert Bitmap to luminance source via Windows compatibility binding
                var luminance = new BitmapLuminanceSource(bmp);
                var result = reader.Decode(luminance);

                if (result == null)
                {
                    return Ok(new { success = false, message = "No barcode detected" });
                }

                // Try to enrich with UPC lookup
                var enrichment = await _upc.LookupAsync(result.Text);

                return Ok(new
                {
                    success = true,
                    barcode = result.Text,
                    format = result.BarcodeFormat.ToString(),
                    suggested = new
                    {
                        Name = enrichment?.Name ?? $"UPC {result.Text}",
                        Brand = enrichment?.Brand,
                        Type = enrichment?.Type ?? "Bourbon",
                        ImageUrl = enrichment?.ImageUrl,
                        Proof = enrichment?.Proof,
                        VolumeMl = enrichment?.VolumeMl,
                        AgeYears = enrichment?.AgeYears
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
