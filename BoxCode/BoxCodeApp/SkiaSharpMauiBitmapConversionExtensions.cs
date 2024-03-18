using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxCodeApp
{
    internal static class SkiaSharpMauiBitmapConversionExtensions
    {
        public static Microsoft.Maui.Graphics.IImage ToMauiImage(this SkiaSharp.SKBitmap bitmap)
        {
            return Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(bitmap.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100).AsStream());
        }
        public static SkiaSharp.SKBitmap ToSkiaSharpBitmap(this Microsoft.Maui.Graphics.IImage image)
        {
            var ms = new MemoryStream();
            image.Save(ms);
            ms.Position = 0;
            return SkiaSharp.SKBitmap.Decode(ms);
        }
    }
}
