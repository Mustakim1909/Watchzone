using SkiaSharp;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Watchzone.Helper
{
    public static class ImageHelper
    {
        // Single method: loads PNG/JPG from server (converted from AVIF/WEBP if needed)
        public static async Task<ImageSource> LoadImageAsync(string url)
        {
            try
            {
                using var client = new HttpClient();
                var bytes = await client.GetByteArrayAsync(url);

                using var bitmap = SKBitmap.Decode(bytes);
                if (bitmap == null)
                    return ImageSource.FromFile("fallback.png");

                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100); // PNG output

                var stream = new MemoryStream();
                data.SaveTo(stream);
                stream.Seek(0, SeekOrigin.Begin);

                return ImageSource.FromStream(() => stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image load failed: {ex.Message}");
                return ImageSource.FromFile("fallback.png");
            }
        }
    }
}
