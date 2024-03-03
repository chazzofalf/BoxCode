namespace BoxCodeLib;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography.X509Certificates;
using SkiaSharp;
internal static class ThothGraphicsExtensions
{
    public static string FromBitmap(this SKBitmap bmp)
    {
        var matrix = Enumerable.Repeat(' ',bmp.Height).Select(s => Enumerable.Repeat(' ',bmp.Width).ToArray()).ToArray();
        ReadBitmap(bmp,matrix).Wait();
        matrix = matrix.Skip(1).Reverse().Skip(1).Reverse().Select( s=> s.Skip(1).Reverse().Skip(1).Reverse().ToArray()).ToArray();
        var x = string.Join("\n",matrix.Select(s => string.Join("",s)));
        return x.UnthothString(true).Trim();
    }
    private static async Task ReadBitmap(SKBitmap bmp,char[][] matrix,int l =-1,int t=-1,int r=-1,int b=-1)
    {
        var lr = r == -1 ? bmp.Width-1 : r;
        var ll = l == -1 ? 0 : l;
        var lb = b == -1 ? bmp.Height-1 : b;
        var lt = t == -1 ? 0 : t;
        int w = (lr+1)-ll;
        int h = (lb+1)-lt;
        int size = w*h;
        if (size > 64)
        {
            await Task.Yield();
            foreach (var y in Enumerable.Range(lt,h))
            {
                foreach (var x in Enumerable.Range(ll,w))
                {
                    if (bmp.GetPixel(x,y) == SKColors.White)
                    {
                        matrix[y][x] = '#';
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
                    ReadBitmap(bmp,matrix,l1,t1,r1,b1),
                    ReadBitmap(bmp,matrix,l2,t2,r2,b2)
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
                    ReadBitmap(bmp,matrix,l1,t1,r1,b1),
                    ReadBitmap(bmp,matrix,l2,t2,r2,b2)
                );
            }
            
        }
    }
    public static SKBitmap CreateBitmap(this string text,bool singleLine=false)
    {
        var etext = text.ThothString(singleLine);
        var etext_split = etext.Split("\n");
        var lines = etext_split.Length;
        var cols = etext_split.Select(s => s.Length).Max();
        var bmp = new SKBitmap(cols+2,lines+2);
        var matrix = Enumerable.Range(0,lines).Select(r => Enumerable.Range(0,cols).Select(c => etext_split[r].Length > c ? etext_split[r][c] : ' ').ToArray()).ToArray();
        
    
        Task.WhenAll(
            WorkOnBitmap(bmp,matrix),
            WorkOnSides(bmp)
        ).Wait();
        
        
        return bmp;
    }
    private static async Task WorkOnSides(SKBitmap bmp)
    {
        await Task.WhenAll(
            TaskWorkOnLeft(bmp),
            TaskWorkOnRight(bmp),
            TaskWorkOnTop(bmp),
            TaskWorkOnBottom(bmp),
            TaskWorkOnCorners(bmp)

        );
    }
    private static async Task TaskWorkOnCorners(SKBitmap bmp)
    {
        await Task.Yield();
        bmp.SetPixel(0,0,SKColors.White);
        bmp.SetPixel(bmp.Width-1,0,SKColors.White);
        bmp.SetPixel(0,bmp.Height-1,SKColors.White);
        bmp.SetPixel(bmp.Width-1,bmp.Height-1,SKColors.White);
    }
    private static async Task TaskWorkOnLeft(SKBitmap bmp)
    {
        await Task.Yield();
        foreach (var idx in Enumerable.Range(1,bmp.Height-2))
        {
            bmp.SetPixel(0,idx,SKColors.Black);
        }
    }
    private static async Task TaskWorkOnRight(SKBitmap bmp)
    {
        await Task.Yield();
        foreach (var idx in Enumerable.Range(1,bmp.Height-2))
        {
            bmp.SetPixel(bmp.Width-1,idx,SKColors.Black);
        }
    }
    private static async Task TaskWorkOnTop(SKBitmap bmp)
    {
        await Task.Yield();
        foreach (var idx in Enumerable.Range(1,bmp.Width-2))
        {
            bmp.SetPixel(idx,0,SKColors.Black);
        }
    }
    private static async Task TaskWorkOnBottom(SKBitmap bmp)
    {
        await Task.Yield();
        foreach (var idx in Enumerable.Range(1,bmp.Width-2))
        {
            bmp.SetPixel(idx,bmp.Height-1,SKColors.Black);
        }
    }
    private static async Task WorkOnBitmap(SKBitmap bmp,char[][] matrix)
    {
        await WorkOnBitmap(bmp,0,0,matrix[0].Length-1,matrix.Length-1,matrix);
    }
    private static async Task WorkOnBitmap(SKBitmap bmp,int l, int t,int r,int b,char[][] matrix)
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
                    if (matrix[y][x] == '#')
                    {
                        bmp.SetPixel(x+1,y+1,SKColors.White);
                    }
                    else
                    {
                        bmp.SetPixel(x+1,y+1,SKColors.Black);
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
                    WorkOnBitmap(bmp,l1,t1,r1,b1,matrix),
                    WorkOnBitmap(bmp,l2,t2,r2,b2,matrix)
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
                    WorkOnBitmap(bmp,l1,t1,r1,b1,matrix),
                    WorkOnBitmap(bmp,l2,t2,r2,b2,matrix)
                );
            }
            
        }
        
    }
}
