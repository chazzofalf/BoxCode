using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Net;
using System.Runtime.CompilerServices;
using SkiaSharp;

namespace BoxCodeLib;

internal static class TovaExtensions
{
    public static SKBitmap TovaBitmap(this string str, bool singleLine=false,SKColor? onColor=null,SKColor? offColor=null,string? hexColorOn=null,string? hexColorOff=null) =>     
    str.GetAntithesis()
    .ConvertToPencodePrestep()
    .ConvertToPencodePoststep()
    .ConvertToGraphics(onColor,offColor,hexColorOn,hexColorOff)
    .AssembleBitmap(singleLine)
    .FinishBitmap()
    .Colorize(onColor,offColor,hexColorOn,hexColorOff);
    public static string TovaFromBitmap(this SKBitmap bmp) 
    {
        var oncolor = bmp.GetPixel(0,0);
        var offcolor = bmp.GetPixel(0,1);
        if (oncolor == offcolor)
        {
            throw new ArgumentException("This is an invalid image.");
        }
        return bmp.Decolorize(oncolor,offcolor)
        .UnfinishBitmap()
        .SplitIntoSegments()
        .ConvertIntoPenRowSequences()
        .ConvertIntoString()
        .GetAntithesis()
        .CoalesceEnumerableToString();
        

    }
    public static T SleepAndPass<T>(this T element,int millis)
    {
        Task.Delay(millis).Wait();
        return element;
    }
    public static string ConvertIntoString(this IEnumerable<string[]> penRowSequences) => penRowSequences
    .Select(s => Pencodes.PenCodeForPenRows(s))
    .Aggregate((Text:"",Inited:false,Finished:false),(prev,cur) => {
        if (!prev.Inited)
        {
            if (cur.IsStart)
            {
                return (Text:"",Inited:true,Finished:false);
            }
            else
            {
                throw new ArgumentException();
            }
        }
        else if (!prev.Finished)
        {
            if (cur.IsEnd)
            {
                return (Text:prev.Text,Inited:true,Finished:true);
            }
            else
            {
                return (Text:prev.Text +cur.Letter,Inited:true,Finished:false);
            }
        }
        else
        {
            return prev;
        }
    },(fin) => {
        return fin.Text;
    });
    public static IEnumerable<string[]> ConvertIntoPenRowSequences(this IEnumerable<SKBitmap> bitmaps) => bitmaps    
    .Select(bmp => Enumerable.Range(0,bmp.Height)
    .Select(r => Enumerable.Range(0,bmp.Width)
    .Select(c => (X:c,Y:r)))
    .Select(crdc => crdc.Select(crd => bmp.GetPixel(crd.X,crd.Y) == SKColors.White ? '#' : ' ').CoalesceEnumerableToString()).ToArray() );
    
    
    private static SKBitmap WriteAndPassThru(this SKBitmap bmp,string? bname=null)
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month;
        var day = now.Day;
        var hour = now.Hour;
        var minute = now.Minute;
        var second = now.Second;
        var micros = now.Microsecond;
        var r = new byte[4];
        System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(r);
        var random = Convert.ToHexString(r);
        var yearStr = $"0000{year}".Reverse().Take(4).Reverse().CoalesceEnumerableToString();
        var monthStr = $"00{month}".Reverse().Take(2).Reverse().CoalesceEnumerableToString();
        var dayStr = $"00{day}".Reverse().Take(2).Reverse().CoalesceEnumerableToString();
        var hourStr = $"00{hour}".Reverse().Take(2).Reverse().CoalesceEnumerableToString();
        var minuteStr = $"00{minute}".Reverse().Take(2).Reverse().CoalesceEnumerableToString();
        var secondStr = $"00{second}".Reverse().Take(2).Reverse().CoalesceEnumerableToString();
        var microsecondStr = $"000000{micros}".Reverse().Take(6).Reverse().CoalesceEnumerableToString();
        var basename = bname != null ? bname : "TovaPNGTest";
        var name = $"{basename}_{yearStr}{monthStr}{dayStr}_{hourStr}{minuteStr}{secondStr}.{microsecondStr}_{random}.png";
        File.WriteAllBytes($"C:\\Temp\\{name}",bmp.Encode(SKEncodedImageFormat.Png,100).ToArray() );
        return bmp;
    }
    public static IEnumerable<SKBitmap> SplitIntoSegments(this SKBitmap bitmap)
    {
        int segWidth = Pencodes.PenCodes.First().PenRowBmpTrimmed.Width;
        int segHeight = Pencodes.PenCodes.First().PenRowBmpTrimmed.Height;
        int segsPerWidth = bitmap.Width/segWidth;
        int segsPerHeight = bitmap.Height/segHeight;
        return Enumerable.Range(0,segsPerHeight)
        .SelectMany(s => Enumerable.Range(0,segsPerWidth)
        .Select(s2 => (X:s2,Y:s)))
        .Select(s => ((Func<SKBitmap>)(() => {
            SKBitmap seg = new SKBitmap(segWidth,segHeight);
            SKCanvas cans = new SKCanvas(seg);
            var srcRect = new SKRect(segWidth*s.X,segHeight*s.Y,segWidth*(s.X+1),segHeight*(s.Y+1));
            var destRect = new SKRect(0,0,segWidth,segHeight);
            
            cans.DrawBitmap(bitmap,new SKRect(segWidth*s.X,segHeight*s.Y,segWidth*(s.X+1),segHeight*(s.Y+1)),new SKRect(0,0,segWidth,segHeight));
            cans.Flush();
            return seg;
        }))()).AsEnumerable();
    }
    public static SKBitmap UnfinishBitmap(this SKBitmap bitmap)
    {
        var unfinishedBmp = new SKBitmap(bitmap.Width-2,bitmap.Height-2);
        var canvas = new SKCanvas(unfinishedBmp);
        canvas.DrawBitmap(bitmap,new SKRect(1,1,bitmap.Width-1,bitmap.Height-1),new SKRect(0,0,unfinishedBmp.Width,unfinishedBmp.Height));
        canvas.Flush();
        return unfinishedBmp;
    }
    public static string TovaString(this string str,bool singleLine=false) {
        var bmp = str.TovaBitmap(singleLine).UnfinishBitmap();
        
        return Enumerable.Range(0,bmp.Height)
            .SelectMany(s => Enumerable.Range(0,bmp.Width)
            .Select(s2 => (X:s2,Y:s)))
            .Select(s => (X:s.X,Y:s.Y,Color:bmp.GetPixel(s.X,s.Y)))
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
        .Concat(Pencodes.LetteredPenCodes
            .Where(pc => pc.Letter[0] == ' '))
        .First());
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
    private static IEnumerable<SKBitmap> ConvertToGraphics(this IEnumerable<PenCode> penCodes,SKColor? onColor,SKColor? offColor,string? hexColorOn,string? hexColorOff) =>
        penCodes
        .Select(pc => pc.PenRowBmpTrimmed)
        ;
    
    private static async Task<SKBitmap> WorkOnBitmapColorize(SKBitmap bmp,SKColor onColor,SKColor offColor)
    {
        var copy = bmp.Copy();
        await WorkOnBitmapColorize(bmp,copy,0,0,bmp.Width-1,bmp.Height-1,onColor,offColor);
        return copy;
    }
    private static async Task WorkOnBitmapColorize(SKBitmap src,SKBitmap dest,int l, int t,int r,int b,SKColor onColor,SKColor offColor)
    {
        int w = (r+1)-l;
        int h = (b+1)-t;
        int size = w*h;
        if (size > 64)
        {
            await Task.Yield();
            foreach (var y in Enumerable.Range(t,h))
            {
                foreach (var x in Enumerable.Range(l,w))
                {
                    if (src.GetPixel(x,y) == SKColors.White)
                    {
                        dest.SetPixel(x,y,onColor);
                    }
                    else if (src.GetPixel(x,y) == SKColors.Black)
                    {
                        dest.SetPixel(x,y,offColor);
                    }       
                    else
                    {
                        throw new ArgumentException();
                    }
                }
            }
        }
        else
        {
            if (w > h)
            {
                var l1 = l;
                var t1 = t;
                var r1 = l+(r-l)/2;
                var b1 = b;
                var l2 = r1+1;
                var t2 = t;
                var r2 = r;
                var b2 = b;
                await Task.WhenAll(
                    WorkOnBitmapColorize(src,dest,l1,t1,r1,b1,onColor,offColor),
                    WorkOnBitmapColorize(src,dest,l2,t2,r2,b2,onColor,offColor)
                );
            }
            else
            {
                var l1 = l;
                var t1 = t;
                var r1 = l;
                var b1 = b+(b-t)/2;
                var l2 = l;
                var t2 = b1+1;
                var r2 = r;
                var b2 = b;
                await Task.WhenAll(
                    WorkOnBitmapColorize(src,dest,l1,t1,r1,b1,onColor,offColor),
                    WorkOnBitmapColorize(src,dest,l2,t2,r2,b2,onColor,offColor)
                );
            }
            
        }
        
    }
    private static async Task<SKBitmap> WorkOnBitmapDecolorize(SKBitmap bmp,SKColor onColor,SKColor offColor)
    {
        var copy = bmp.Copy();
        await WorkOnBitmapDecolorize(bmp,copy,0,0,bmp.Width-1,bmp.Height-1,onColor,offColor);
        return copy;
    }
    private static async Task WorkOnBitmapDecolorize(SKBitmap src,SKBitmap dest,int l, int t,int r,int b,SKColor onColor,SKColor offColor)
    {
        int w = (r+1)-l;
        int h = (b+1)-t;
        int size = w*h;
        if (size > 64)
        {
            await Task.Yield();
            foreach (var y in Enumerable.Range(t,h))
            {
                foreach (var x in Enumerable.Range(l,w))
                {
                    if (src.GetPixel(x,y) == onColor)
                    {
                        dest.SetPixel(x,y,SKColors.White);
                    }
                    else if (src.GetPixel(x,y) ==  offColor)
                    {
                        dest.SetPixel(x,y,SKColors.Black);
                    }
                    else
                    {
                        throw new ArgumentException("This image is invalid.");
                    }
                }
            }
        }
        else
        {
            if (w > h)
            {
                var l1 = l;
                var t1 = t;
                var r1 = l+(r-l)/2;
                var b1 = b;
                var l2 = r1+1;
                var t2 = t;
                var r2 = r;
                var b2 = b;
                await Task.WhenAll(
                    WorkOnBitmapDecolorize(src,dest,l1,t1,r1,b1,onColor,offColor),
                    WorkOnBitmapDecolorize(src,dest,l2,t2,r2,b2,onColor,offColor)
                );
            }
            else
            {
                var l1 = l;
                var t1 = t;
                var r1 = l;
                var b1 = b+(b-t)/2;
                var l2 = l;
                var t2 = b1+1;
                var r2 = r;
                var b2 = b;
                await Task.WhenAll(
                    WorkOnBitmapDecolorize(src,dest,l1,t1,r1,b1,onColor,offColor),
                    WorkOnBitmapDecolorize(src,dest,l2,t2,r2,b2,onColor,offColor)
                );
            }
            
        }
        
    }
    private static SKBitmap Decolorize(this SKBitmap bitmap,SKColor onColor,SKColor offColor)
    {
        return WorkOnBitmapDecolorize(bitmap,onColor,offColor).Result;
    }
    
    private static SKBitmap Colorize(this SKBitmap bitmap,SKColor? onColor,SKColor? offColor,string? hexColorOn,string? hexColorOff)
    {
        SKColor colorOn = SKColors.White;
        SKColor colorOff = SKColors.Black;
        
        if (hexColorOn != null)
        {
            var hexcolor = "";
            if (hexColorOn.StartsWith('#'))
            {
                hexcolor = hexColorOn.Substring(1);
            }
            else
            {
                hexcolor = hexColorOn;
            }
            
            var colorbytes = Convert.FromHexString(hexcolor);
            (var red,var green,var blue) = (colorbytes[0],colorbytes[1],colorbytes[2]);
            colorOn = new SKColor(red,green,blue);
        }
        else if (onColor != null)
        {
            colorOn = onColor.Value;
        }
        if (hexColorOff != null)
        {
            var hexcolor = "";
            if (hexColorOff.StartsWith('#'))
            {
                hexcolor = hexColorOff.Substring(1);
            }
            else
            {
                hexcolor = hexColorOff;
            }
            
            var colorbytes = Convert.FromHexString(hexcolor);
            (var red,var green,var blue) = (colorbytes[0],colorbytes[1],colorbytes[2]);
            colorOff = new SKColor(red,green,blue);
        }
        else if (offColor != null)
        {
            colorOff = offColor.Value;
        }
        if (colorOn != colorOff)
        {
            return WorkOnBitmapColorize(bitmap,colorOn,colorOff).Result;
        }
        else
        {
            throw new ArgumentException($"You must choose two DISTINCT colors!!! You have chosen the following color twice!:(R:{colorOn.Red},G:{colorOn.Green},B:{colorOn.Blue})");
        }
    }
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
        canvas.DrawRect(new SKRect(0,0,output.Width,output.Height),paint);
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
            canvas.DrawBitmap(oneliner_bmp,new SKRect(owidth*cur,0,owidth*cur+owidth,height),new SKRect(0,height*cur,owidth,height*cur+height));
            return y;
        });
        canvas.Flush();
        return output;

    }
    
}
