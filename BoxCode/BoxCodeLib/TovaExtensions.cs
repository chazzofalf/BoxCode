using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using SkiaSharp;

namespace BoxCodeLib;

internal static class TovaExtensions
{
    public static bool TovaIsSingleLine(this SKBitmap bitmap) => bitmap.TovaIsValid() ? bitmap.IsSingleLine() : false;

    private static SKBitmap TovaBitmapMultiline(this string str,SKColor? onColor=null,SKColor? offColor=null,string? hexColorOn=null,string? hexColorOff=null)
    {
        return Regex.Split(str, "\r\n|\r|\n")
            .Select(s => s.TovaBitmap(singleLine: true))
            .AssembleBitmapMultiline()
            .FinishBitmap(bright:false)
            .Colorize(onColor:onColor,offColor:offColor,hexColorOn:hexColorOn,hexColorOff:hexColorOff);

    }
    private static SKBitmap AssembleBitmapMultiline(this IEnumerable<SKBitmap> bitmapLines)
    {
        var segHeight = bitmapLines.First().Height;
        var totalHeight = segHeight * bitmapLines.Count();
        var totalWidth = bitmapLines
            .Select(s => s.Width)
            .Max();
        var bmp = new SKBitmap(totalWidth, totalHeight);
        var canvas = new SKCanvas(bmp);
        bitmapLines.Aggregate(0, (y, current) =>
        {
            var lineHeight = current.Height;
            var lineWidth = current.Width;
            var xPos = (totalWidth - lineWidth) / 2;
            canvas.DrawBitmap(current, new SKPoint(x: xPos, y: y));
            return y+lineHeight;
        });
        canvas.Flush();
        return bmp;
    }
    public static bool IsMultiline(this SKBitmap bitmap)
    {
        return 1 < Enumerable.Range(0, bitmap.Height)
            .SelectMany(y => Enumerable.Range(0, bitmap.Width)
            .Select(x => (X: x, Y: y)))
            .Aggregate((LinesCounted:0,TopLeft:((int X,int Y)?)null,TopRight:((int X,int Y)?)null,BottomLeft:((int X,int Y)?)null,BottomRight:((int X,int Y)?)null),(state,currentCoordinate) =>
            {
                // Time to turn in for the night. I AM HERE!!! TODO: Start from here.
                return state;
            },(fin) => fin.LinesCounted);
    }
    public static SKBitmap TovaBitmap(this string str, bool singleLine=false,SKColor? onColor=null,SKColor? offColor=null,string? hexColorOn=null,string? hexColorOff=null) =>
    Regex.Split(str, "\r\n|\r|\n").Length > 1 ? str.TovaBitmapMultiline(onColor:onColor,offColor:offColor,hexColorOn:hexColorOn,hexColorOff:hexColorOff) :
    str.GetAntithesis()
    .ConvertToPencodePrestep()
    .ConvertToPencodePoststep()
    .ConvertToGraphics(onColor,offColor,hexColorOn,hexColorOff)
    .AssembleBitmap(singleLine)    
    .FinishBitmap()
    .Colorize(onColor,offColor,hexColorOn,hexColorOff);

    
    public static bool TovaIsValid(this SKBitmap bitmap, string[]? output = null) => bitmap.TovaIsValidSingleLine(output: output) || bitmap.TovaIsValidMultiline(output:output);

    private static bool TovaIsValidMultiline(this SKBitmap bitmap, string[]? output=null)
    {
        try
        {
            var outx = bitmap.TovaFromBitmapMultiLine();
            if (output != null && output.Length == 1)
            {
                output[0] = outx;
            }
            return true;
        }
        catch 
        {
            return false;
        }
    }
    private static bool TovaIsValidSingleLine (this SKBitmap bitmap, string[]? output=null)
    {
        try
        {
            var outx = TovaFromBitmapSingleLine(bitmap);
            if (output != null && output.Length == 1)
            {
                output[0] = outx;
            }
            return true;
        }
        catch 
        {
            return false;
        }
    }

    public static string TovaFromBitmap(this SKBitmap bmp)
    {
        string[] output = new string[1];
        if (bmp.TovaIsValidSingleLine(output))
        {
            return output[0];
        }
        else if (bmp.TovaIsValidMultiline(output))
        {
            return output[0];
        }
        throw new ArgumentException();
        
    }

    public static string TovaFromBitmapMultiLine(this SKBitmap bmp) => 
            string.Join("\n",Enumerable.Range(0,bmp.Height)
            .SelectMany(y => Enumerable.Range(0,bmp.Width)
            .Select(x => (X:x,Y:y)))
            .Aggregate((Bitmaps:Enumerable.Empty<SKBitmap>(),BackColor:(SKColor?)null,ForeColor:(SKColor?)null,TopLeft:((int X,int Y)?)null,TopRight:((int X,int Y)?)null,BottomLeft:((int X,int Y)?)null,BottomRight:((int X,int Y)?)null),
            (state,coordinate) => {
                if (state.BackColor == null)
                {
                    return (Bitmaps: state.Bitmaps,
                    BackColor: bmp.GetPixel(coordinate.X, coordinate.Y),
                    ForeColor: state.ForeColor,
                    TopLeft: state.TopLeft,
                    TopRight: state.TopRight,
                    BottomLeft: state.BottomLeft,
                    BottomRight: state.BottomRight
                    );
                }
                else
                {
                    if (coordinate.Y == 0 || coordinate.Y == bmp.Height-1 || coordinate.X == 0 || coordinate.X == bmp.Width-1)
                    {
                        if (bmp.GetPixel(coordinate.X,coordinate.Y) != state.BackColor)
                        {
                            throw new ArgumentException("Edges must be a uniform color.");
                        }
                        else
                        {
                            return state;
                        }
                    }
                    else
                    {
                        if (state.ForeColor == null)
                        {
                            if (bmp.GetPixel(coordinate.X,coordinate.Y) != state.BackColor)
                            {
                                return (Bitmaps: state.Bitmaps,
                                BackColor: state.BackColor,
                                ForeColor: bmp.GetPixel(coordinate.X, coordinate.Y),
                                TopLeft: coordinate,
                                TopRight: state.TopRight,
                                BottomLeft: state.BottomLeft,
                                BottomRight: state.BottomRight);
                            }
                            else
                            {
                                return state;
                            }
                        }
                        else
                        {
                            if (state.TopLeft == null)
                            {
                                if (bmp.GetPixel(coordinate.X,coordinate.Y) == state.BackColor)
                                {
                                    return state;
                                }
                                else if (bmp.GetPixel(coordinate.X,coordinate.Y) == state.ForeColor)
                                {
                                    return (Bitmaps: state.Bitmaps,
                                    BackColor:state.BackColor,
                                    ForeColor:state.ForeColor,
                                    TopLeft: coordinate,
                                    TopRight: state.TopRight,
                                    BottomLeft: state.BottomLeft,
                                    BottomRight: state.BottomRight);
                                }
                                else
                                {
                                    throw new Exception("Tova Bitmaps should only have two colors!");
                                }    
                            }
                            else
                            {
                                if (state.TopRight == null)
                                {
                                    if (bmp.GetPixel(coordinate.X,coordinate.Y) == state.BackColor)
                                    {
                                        return state;
                                    }
                                    else if (bmp.GetPixel(coordinate.X,coordinate.Y) == state.ForeColor)
                                    {
                                        if (coordinate.Y == state.TopLeft.Value.Y)
                                        {
                                            return (Bitmaps: state.Bitmaps,
                                            BackColor:state.BackColor,
                                            ForeColor:state.ForeColor,
                                            TopLeft:state.TopLeft,
                                            TopRight:coordinate,
                                            BottomLeft:state.BottomLeft,
                                            BottomRight:state.BottomRight);
                                        }
                                        else
                                        {
                                            throw new Exception("There seems to be no right edge to this line segment. This will not do.");
                                        }    
                                    }
                                    else
                                    {
                                        throw new Exception("Tova Bitmaps should only have two colors!");
                                    }
                                }
                                else
                                {
                                    if (state.BottomLeft == null)
                                    {
                                        if (bmp.GetPixel(coordinate.X, coordinate.Y) == state.BackColor)
                                        {
                                            return state;
                                        }
                                        else if (bmp.GetPixel(coordinate.X, coordinate.Y) == state.ForeColor)
                                        {
                                            if (coordinate.Y == state.TopRight.Value.Y)
                                            {
                                                throw new Exception("This line segment already has a right edge!");
                                            }
                                            else if (coordinate.Y > state.TopRight.Value.Y)
                                            {
                                                if (coordinate.X < state.TopLeft.Value.X)
                                                {
                                                    throw new Exception("Subsequent Rows start outside the box!");
                                                }
                                                else if (coordinate.X == state.TopLeft.Value.X)
                                                {
                                                    return (Bitmaps: state.Bitmaps,
                                                    BackColor: state.BackColor,
                                                    ForeColor: state.ForeColor,
                                                    TopLeft: state.TopLeft,
                                                    TopRight: state.TopRight,
                                                    BottomLeft: coordinate,
                                                    state.BottomRight);
                                                }
                                                else if (coordinate.X >= state.TopRight.Value.X)
                                                {
                                                    throw new Exception("Subsequent Rows end outside the box!");
                                                }
                                                else
                                                {
                                                    return state;
                                                }
                                            }
                                            else
                                            {
                                                throw new Exception("Subsequent Rows are before the first? Is the bitmap enumerator setup correctly?");
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("Tova Bitmaps should only have two colors!");
                                        }
                                    }
                                    else
                                    {
                                        if (state.BottomRight == null)
                                        { 
                                            if (bmp.GetPixel(coordinate.X,coordinate.Y) == state.BackColor)
                                            {
                                                return state;
                                            }
                                            else if (bmp.GetPixel(coordinate.X,coordinate.Y) == state.ForeColor)
                                            {
                                                if (coordinate.Y == state.BottomLeft.Value.Y)
                                                {
                                                    if (coordinate.X == state.TopRight.Value.X)
                                                    {
                                                        var rect = new SKRect(left: state.TopLeft.Value.X, top: state.TopLeft.Value.Y, right: state.TopRight.Value.X+1 , bottom: coordinate.Y+1 );
                                                        var bitmap = new SKBitmap((int)rect.Width, (int)rect.Height);
                                                        var canvas = new SKCanvas(bitmap);
                                                        canvas.DrawBitmap(bmp,rect,new SKRect(0,0, bitmap.Width, bitmap.Height));
                                                        canvas.Flush();                                                        
                                                        return (Bitmaps: state.Bitmaps.Append(bitmap),
                                                        BackColor: state.BackColor,
                                                        ForeColor: state.ForeColor,
                                                        TopLeft: null,
                                                        TopRight: null,
                                                        BottomLeft: null,
                                                        BottomRight: null);
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("Square is not even.");
                                                    }
                                                }
                                                else
                                                {
                                                    throw new Exception("Square is not even.");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("State should have reset the corners and added a bitmap by now!");
                                        }
                                    }
                                }
                            }
                        }    
                    }
                }    
                return state;
        },(fin) => fin.Bitmaps)
            .Select(bmp => bmp.TovaFromBitmapSingleLine()));
    
    public static string TovaFromBitmapSingleLine(this SKBitmap bmp) 
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
    private static T SleepAndPass<T>(this T element,int millis)
    {
        Task.Delay(millis).Wait();
        return element;
    }
    private static string ConvertIntoString(this IEnumerable<string[]> penRowSequences) => penRowSequences
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
    private static IEnumerable<string[]> ConvertIntoPenRowSequences(this IEnumerable<SKBitmap> bitmaps) => bitmaps    
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
    public static bool IsSingleLine(this SKBitmap bitmap)
    {
        int segWidth = Pencodes.PenCodes.First().PenRowBmpTrimmed.Width;
        int segHeight = Pencodes.PenCodes.First().PenRowBmpTrimmed.Height;
        int segsPerWidth = bitmap.Width / segWidth;
        int segsPerHeight = bitmap.Height / segHeight;
        return segsPerHeight == 1;
    }
    private static IEnumerable<SKBitmap> SplitIntoSegments(this SKBitmap bitmap)
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
    private static SKBitmap UnfinishBitmap(this SKBitmap bitmap)
    {
        var unfinishedBmp = new SKBitmap(bitmap.Width-2,bitmap.Height-2);
        var canvas = new SKCanvas(unfinishedBmp);
        canvas.DrawBitmap(bitmap,new SKRect(1,1,bitmap.Width-1,bitmap.Height-1),new SKRect(0,0,unfinishedBmp.Width,unfinishedBmp.Height));
        canvas.Flush();
        return unfinishedBmp;
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
    .Concat(penCodes.Reverse()
        .Select(pc => pc.GetReversedPencode()))
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
    private static SKBitmap FinishBitmap(this SKBitmap bitmap,bool bright=true) 
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
        output.SetPixel(0,0,bright ? SKColors.White : SKColors.Black);
        output.SetPixel(output.Width-1,0, bright ? SKColors.White : SKColors.Black);
        output.SetPixel(0,output.Height-1, bright ? SKColors.White : SKColors.Black);
        output.SetPixel(output.Width-1,output.Height-1, bright ? SKColors.White : SKColors.Black);
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
