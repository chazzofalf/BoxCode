using SkiaSharp;

namespace BoxCodeLib;

public static class LibraryUtil
{
    public static SKBitmap ConvertToBitmap(string text,bool singleLine=true,SKColor? foreground=null,SKColor? background=null)
    {
        return text.TovaBitmap(singleLine:singleLine,onColor:foreground == null ? SKColors.White : foreground,offColor:background == null ? SKColors.Black : background);
    }
    public static string ConvertFromBitmap(SKBitmap bitmap)
    {
        return bitmap.TovaFromBitmap();
    }
    public static bool IsValid(SKBitmap bitmap, string[]? output=null)
    {
        return bitmap.TovaIsValid(output);
    }
    public static bool IsSingleLine(SKBitmap bitmap)
    {
        return bitmap.TovaIsSingleLine();
    }
}
