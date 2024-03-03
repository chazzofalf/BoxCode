using SkiaSharp;

namespace BoxCode;

public class PenCode
{
    public string Letter {get;  }
    public string[] PenRows {get;  }

    public bool IsSpecial {get; }
    public bool IsStart {get; }
    public bool IsReverseStart {get; }
    public bool IsEnd {get; }
    public bool IsUsed {get; }

    internal PenCode(RawPenCode rpc) {
        if (rpc.Letter != null && rpc.PenRows != null
        && rpc.IsSpecial != null
        && rpc.IsStart != null
        && rpc.IsReverseStart != null
        && rpc.IsEnd != null
        && rpc.IsUsed != null)
        {
            Letter= rpc.Letter;
            PenRows = rpc.PenRows;
            IsSpecial = rpc.IsSpecial.Value;
            IsStart = rpc.IsStart.Value;
            IsReverseStart = rpc.IsReverseStart.Value;
            IsEnd = rpc.IsEnd.Value;
            IsUsed = rpc.IsUsed.Value;

            
        }
        else
        {
            throw new ArgumentException();
        }
    }

    private static async Task WorkOnBitmap(SKBitmap bmp,char[][] matrix,bool trimmed=false)
    {
        await WorkOnBitmap(bmp,0,0,matrix[0].Length-1,matrix.Length-1,matrix,trimmed);
    }
    private static async Task WorkOnBitmap(SKBitmap bmp,int l, int t,int r,int b,char[][] matrix,bool trimmed)
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
                        bmp.SetPixel(x+(! trimmed ? 1 : 0),y+(! trimmed ? 1 : 0),SKColors.White);
                    }
                    else
                    {
                        bmp.SetPixel(x+(! trimmed ? 1 : 0),y+(! trimmed ? 1 : 0),SKColors.Black);
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
                    WorkOnBitmap(bmp,l1,t1,r1,b1,matrix,trimmed),
                    WorkOnBitmap(bmp,l2,t2,r2,b2,matrix,trimmed)
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
                    WorkOnBitmap(bmp,l1,t1,r1,b1,matrix,trimmed),
                    WorkOnBitmap(bmp,l2,t2,r2,b2,matrix,trimmed)
                );
            }
            
        }
        
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
        bmp.SetPixel(0,0,SKColors.Black);
        bmp.SetPixel(bmp.Width-1,0,SKColors.Black);
        bmp.SetPixel(0,bmp.Height-1,SKColors.Black);
        bmp.SetPixel(bmp.Width-1,bmp.Height-1,SKColors.Black);
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
    public string PenRowsStr => string.Join("\n",PenRows);
    public SKBitmap PenRowBmpTrimmed 
    {
        get
        {
            var lines = PenRows.Length;
        var cols = PenRows.Select(s => s.Length).Max();
            var bmp = new SKBitmap(cols,lines);
        var matrix = Enumerable.Range(0,lines).Select(r => Enumerable.Range(0,cols).Select(c => PenRows[r].Length > c ? PenRows[r][c] : ' ').ToArray()).ToArray();
        
    
        Task.WhenAll(
            WorkOnBitmap(bmp,matrix,true)            
        ).Wait();
        return bmp;
        }
    }
    public SKBitmap PenRowsBmp {
        get
        {
            var lines = PenRows.Length;
        var cols = PenRows.Select(s => s.Length).Max();
            var bmp = new SKBitmap(cols+2,lines+2);
        var matrix = Enumerable.Range(0,lines).Select(r => Enumerable.Range(0,cols).Select(c => PenRows[r].Length > c ? PenRows[r][c] : ' ').ToArray()).ToArray();
        
    
        Task.WhenAll(
            WorkOnBitmap(bmp,matrix),
            WorkOnSides(bmp)
        ).Wait();
        return bmp;
        }
        
    }
    
    internal PenCode(string letter,string[] penRows)
    {
        Letter = letter;
        PenRows = penRows;
    }
}
