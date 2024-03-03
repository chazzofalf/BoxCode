using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Net;
using System.Runtime.CompilerServices;
using SkiaSharp;

namespace BoxCode;

public static class TovaExtensions
{
    public static SKBitmap TovaBitmap(this string str, bool singleLine=false) => 
    str.GetAntithesis()
    .ConvertToPencodePrestep()
    .ConvertToPencodePoststep()
    .ConvertToGraphics()
    .AssembleBitmap(singleLine)
    .FinishBitmap();

    public static string TovaString(this string str,bool singleLine=false) {
        var bmp = str.TovaBitmap(singleLine);
        var unfinishedBmp = new SKBitmap(bmp.Width-1,bmp.Height-1);
        var canvas = new SKCanvas(unfinishedBmp);
        canvas.DrawBitmap(bmp,new SKRect(1,1,bmp.Width-2,bmp.Height-2),new SKRect(0,0,unfinishedBmp.Width-1,unfinishedBmp.Height-1));
        canvas.Flush();
        return Enumerable.Range(0,unfinishedBmp.Height)
            .SelectMany(s => Enumerable.Range(0,unfinishedBmp.Width)
            .Select(s2 => (X:s2,Y:s)))
            .Select(s => (X:s.X,Y:s.Y,Color:unfinishedBmp.GetPixel(s.X,s.Y)))
            .Select(s => (X:s.X,Y:s.Y,Ch:s.Color == SKColors.White ? '#' : ' '))
            .GroupBy(s => s.Y)
            .Select(s => s.OrderBy(s2 => s2.X).Select(s2 => s2.Ch))
            .Select(s => s.CoalesceEnumerableToString())
            .CoalesceEnumerableToLinedString();
        
    }
    
    
    
    // public static string UnthothString(this string str,bool padded=false) => 
    // string.Join("",str.ThothAnticode(padded).ToArray());
    
    public static string CoalesceEnumerableToLinedString(this IEnumerable<string> strs) => string.Join("\n",strs);
    public static string CoalesceEnumerableToString(this IEnumerable<char> chars) => string.Join("",chars);
    private static IEnumerable<PenCode> ConvertToPencodePrestep(this IEnumerable<char> antiText) =>
    
        antiText
        .Select(ch => Pencodes.LetteredPenCodes
        .Where(pc => pc.Letter[0] == ch)
        .Select(pc => pc)
        .Single());
    private static IEnumerable<PenCode> ConvertToPencodePoststep(this IEnumerable<PenCode> penCodes) =>
    Pencodes.PenCodes
    .Where(pc => pc.IsSpecial)
    .Where(pc => pc.IsStart)
    .Concat(penCodes)
    .Concat(Pencodes.PenCodes
    .Where(pc => pc.IsSpecial)
    .Where(pc => pc.IsEnd))
    .Concat(penCodes.Reverse())
    .Concat(Pencodes.PenCodes
    .Where(pc => pc.IsSpecial)
    .Where(pc => pc.IsReverseStart));
    private static IEnumerable<SKBitmap> ConvertToGraphics(this IEnumerable<PenCode> penCodes) =>
        penCodes
        .Select(pc => pc.PenRowsBmp);
    private static SKBitmap AssembleBitmap(this IEnumerable<SKBitmap> bitmaps,bool singleLine) => singleLine ? 
    bitmaps.AssembleSingleLineBitmap() : 
    bitmaps.AssembleSquareBitmap();
    private static SKBitmap FinishBitmap(this SKBitmap bitmap) 
    {
        var output = new SKBitmap(bitmap.Width+2,bitmap.Height+2);
        var canvas = new SKCanvas(output);
        var paint = new SKPaint
        {
            Color = SKColors.Black
        };
        canvas.DrawRect(new SKRect(0,0,bitmap.Width-1,bitmap.Height-1),paint);
        canvas.DrawBitmap(bitmap,new SKPoint(1,1));        
        canvas.Flush();
        output.SetPixel(0,0,SKColors.White);
        output.SetPixel(output.Width-1,0,SKColors.White);
        output.SetPixel(0,output.Height-1,SKColors.White);
        output.SetPixel(output.Width-1,output.Height-1,SKColors.White);
        return output;
    }

    private static SKBitmap AssembleSingleLineBitmap(this IEnumerable<SKBitmap> bitmaps)
    {
        var bmp = new SKBitmap(bitmaps.First().Width*bitmaps.Count(),bitmaps.First().Height);
        var canvas = new SKCanvas(bmp);
        bitmaps.Aggregate(0,(x,cur) => 
        {
            canvas.DrawBitmap(cur,new SKPoint(x,0));
            return x+cur.Width;
        });
        canvas.Flush();
        return bmp;
    }

    private static SKBitmap AssembleSquareBitmap(this IEnumerable<SKBitmap> bitmaps) 
    {
        var oneliner_bmp = bitmaps.AssembleSingleLineBitmap();
        var width = bitmaps.First().Width;
        var height = bitmaps.First().Height;
        var cwidth = oneliner_bmp.Width / width;
        var clinespan = (int)Math.Floor(Math.Sqrt(cwidth));
        var cheight = (int)Math.Ceiling((double)cwidth/(double)clinespan);
        var owidth = clinespan*width;
        var oheight = cheight*height;
        var output = new SKBitmap(owidth,oheight);
        var canvas = new SKCanvas(output);
        Enumerable.Range(0,cheight)
        .Aggregate(0,(y,cur) => 
        {
            canvas.DrawBitmap(oneliner_bmp,new SKRect(owidth*cur,0,owidth*cur+owidth-1,height),new SKRect(0,height*cur,owidth-1,height*cur+height-1));
            return y;
        });
        canvas.Flush();
        return output;

    }
    
}
