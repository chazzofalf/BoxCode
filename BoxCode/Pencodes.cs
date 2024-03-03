using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace BoxCode;

public static class Pencodes
{
    private static PenCode[]? _pencodes = null;
    private static PenCode[]? _sideSpacedPenCodes = null;
    private static PenCode[]? _centerSlattedPenCodes = null;

    private static PenCode[]? _squaredPenCodes = null;

    public static PenCode[] SquaredPenCodes => _squaredPenCodes ??= GetSquaredPenCodes();

    public static PenCode[] CenterSlattedPenCodes => _centerSlattedPenCodes ??= GetCenterSlattedPenCodes();

    public static PenCode ToCenterSlattedFromSquared(this PenCode squared) => new PenCode(letter:squared.Letter,penRows:squared.PenRows
    .Reverse()
    .Skip(1)
    .Reverse()
    .ToArray());

    public static PenCode ToSideSpacedFromCenterSlatted(this PenCode slatted) => new PenCode(letter:slatted.Letter,penRows:slatted.PenRows
    .Select(s => s.Substring(0,s.Length/2-1) + s.Substring(s.Length/2))
    .ToArray());

    public static PenCode ToNormalFromSideSpaced(this PenCode sideSpaced) => new PenCode(letter:sideSpaced.Letter,penRows:sideSpaced.PenRows
    .Select(s => string.Join("", s.Skip(1).Reverse().Skip(1).Reverse())).ToArray());

    public static PenCode UnwarpPencode(this PenCode warped) => warped
    .ToCenterSlattedFromSquared().
    ToSideSpacedFromCenterSlatted()
    .ToNormalFromSideSpaced();
    
    private static PenCode[] GetCenterSlattedPenCodes() => SideSpacedPenCodes
        .Select(s => new PenCode(letter:s.Letter,penRows:s.PenRows
        .Select(s2 => s2.Substring(0,s2.Length/2) + s2[s2.Length/2] + s2[s2.Length/2] + s2.Substring(s2.Length/2+1)).ToArray())).ToArray();
    
    private static PenCode[] GetSquaredPenCodes() => CenterSlattedPenCodes
    .Select(s => new PenCode(letter:s.Letter,s.PenRows
    .Append(string.Join("",Enumerable.Repeat(" ",CenterSlattedPenCodes.First().PenRows.First().Count()))).ToArray())).ToArray();

    public static PenCode PenCodeForPenRows(this string[] penrows)
    {
        var x = Pencodes.PenCodes.Select(pc => (PenCode:pc,PenRowPairs:pc.PenRows.Zip(penrows,(a,b) => (PenRowA:a,PenRowB:b))));
        var y = x.Select(s => (PenCode:s.PenCode,MatchTests:s.PenRowPairs.Select(s2 => s2.PenRowA == s2.PenRowB)));
        var z = y.Select(s => (PenCode:s.PenCode,Match:!s.MatchTests.Where(s2 => !s2).Any()));
        var a = z.Where(s => s.Match).Select(s => s.PenCode).Single();
        return a;
    }
    public static char LetterForPenRows(this string[] penrows) =>
    
        penrows.PenCodeForPenRows().Letter[0];
        

    public static PenCode[] SideSpacedPenCodes =>  _sideSpacedPenCodes ??= GetSideSpacedPenCodes();
    
    private static PenCode[] GetSideSpacedPenCodes() =>    
        PenCodes
        .Select(s => new PenCode(letter:s.Letter,penRows:s.PenRows
        .Select(s2 => $" {s2} ")
        .ToArray()))
        .ToArray();
    

    public static PenCode[] PenCodes => _pencodes ??= GetPenCodes();

    public static PenCode[] LetteredPenCodes => PenCodes
    .Where(pc => !pc.IsSpecial)
    .Where(pc => pc.Letter.Length == 1)
    .ToArray();

    public static PenCode[] GetPenCodes()
    {
        return GetPrePenCodes()        
        .ToArray();
    }
    private static PenCode[] GetPrePenCodes()
    {
        return GetRawPenCodes()
        .Select((pc) => new PenCode(pc)).ToArray();
    }
    private static RawPenCode[] GetRawPenCodes()
    {
        var ass = typeof(Pencodes).Assembly;
        var st = typeof(Pencodes).Assembly.GetManifestResourceStream("Pencode");
        var s = "";
        if (st != null)
        {
            using (var tr = new StreamReader(st,Encoding.UTF8))
            {
                s = tr.ReadToEnd();
            }
        }
        else
        {
            throw new ArgumentException();
        }
        var obj = JsonConvert.DeserializeObject<RawPenCode[]>(s);
        if (obj != null)
        {
            return obj;
        }
        else
        {
            throw new ArgumentException();
        }
    }
}
