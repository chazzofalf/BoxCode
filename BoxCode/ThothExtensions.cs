using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;

namespace BoxCode;

public static class ThothExtensions
{
    private static IEnumerable<char> Vowels => "aeiouy";

    private static IEnumerable<char> NumberVowels = "369";
    
    private static IEnumerable<char> Specials = "bc";

    private static IEnumerable<char> AntiSpecials = "zx";

    private static IEnumerable<char> Numbers => All
        .Where((ch) => ch.IsNumber())
        .Where(char.IsAscii);
    
    private static bool IsNumberVowel(this char letter) => NumberVowels
        .Where((ch) => ch == letter)
        .Any();

    private static bool IsNumberConsonant(this char letter) => letter.IsNumber() && !letter.IsNumberVowel();

    private static IEnumerable<char> NumberConsonants => Numbers
    .Where((ch) => ch.IsNumberConsonant());

    private static bool IsSpecial(this char letter) => Specials
        .Where((ch) => ch == letter.ToLower())
        .Any();
    
    private static bool IsAntiSpecial(this char letter) => AntiSpecials
    .Where((ch) => ch == letter.ToLower())
    .Any();

    private static bool IsPencodeLetter(this char letter) => PencodeLetters
    .Where((ch) => ch == letter)
    .Any();

    

    private static int GetSpecialIndex(this char letter) => Specials
    .Zip(Enumerable.Range(0,int.MaxValue),(ch,index) => (Character:ch,Index:index))
    .Where((ch) => ch.Character == letter.ToLower())
    .Select((ch) => ch.Index)
    .First();

    private static int GetAntiSpecialIndex(this char letter) => AntiSpecials
    .Zip(Enumerable.Range(0,int.MaxValue),(ch,index) => (Character:ch,Index:index))
    .Where((ch) => ch.Character == letter.ToLower())
    .Select((ch) => ch.Index)
    .First();

    private static char GetAntiSpecial(this char letter) => letter.IsSpecial() ?
            letter.IsUpper() ?
                AntiSpecials.Skip(letter.GetSpecialIndex()).First().ToUpper() 
            :
                AntiSpecials.Skip(letter.GetSpecialIndex()).First() 
        :
        letter.IsAntiSpecial() ?
            letter.IsUpper() ?
                Specials.Skip(letter.GetAntiSpecialIndex()).First().ToUpper() 
            :
                Specials.Skip(letter.GetAntiSpecialIndex()).First() 
        :        
        letter;

    private static bool IsVowel(this char letter) => Vowels
        .Where((ch) =>  ch == letter.ToLower())
        .Any();
    private static int SqrtCeilingSize(this IEnumerable<char> str) => (int)Math.Ceiling(Math.Sqrt(str.Count()));

    private static int MinimumDimensionSize(this IEnumerable<char> str) 
    {
        var sd = SqrtCeilingSize(str);
        return Math.Max(8,sd);
    }
    private static IEnumerable<IEnumerable<char>> Lines(this IEnumerable<char> str) => str.Chunk(str.MinimumDimensionSize());

    private static string LinedStr(this IEnumerable<IEnumerable<char>> lines) => string.Join("\n",lines.Select(line => string.Join("",line)));
    
    private static string UnpennedString(this string pennedStr)
    {
        var cwidth = Pencodes.PenCodes.First().PenRows.First().Length;
        var cheight = Pencodes.PenCodes.First().PenRows.Count();
        var x15 = pennedStr;
        var x14 =  x15.Split("\n");
        var x13 = x14.Select(s => s.Chunk(cwidth).Select(s2 => string.Join("",s2)).ToArray()).ToArray();
        var x12 = x13.Chunk(cheight).ToArray();
        var x11 = x12.Zip(Enumerable.Range(0,int.MaxValue),(line,rowIndex) => (RowIndex:rowIndex,Line:line));
        var x10 = x11.Select(s=> (RowIndex:s.RowIndex,Row:s.Line.Zip(Enumerable.Range(0,int.MaxValue),(cs,csi) => (ColumnSegmentIndex:csi,Columns:cs))));
        var x9 = x10.Select(s => (RowIndex:s.RowIndex,Row:s.Row
        .Select(s2 => (ColumnSegmentIndex:s2.ColumnSegmentIndex,Columns:s2.Columns
        .Zip(Enumerable.Range(0,int.MaxValue),(columnSegment,columnIndex) => (ColumnIndex:columnIndex,columnSegment:columnSegment))))));
        var x8 = x9.Select(s => (RowIndex:s.RowIndex,Row:s.Row.SelectMany(s2 => s2.Columns.Select(s3 => (ColumnIndex:s3.ColumnIndex,ColumnSegmentIndex:s2.ColumnSegmentIndex,ColumnSegment:s3.columnSegment)))));
        var x7 = x8.SelectMany(s => s.Row.Select(s2 => (RowIndex:s.RowIndex,ColumnIndex:s2.ColumnIndex,ColumnSegmentIndex:s2.ColumnSegmentIndex,ColumnSegment:s2.ColumnSegment)));
        var x6 = x7.GroupBy(x => x.RowIndex).Select(s => (RowIndex:s.Key,Row:s.Select(s2 => (ColumnIndex:s2.ColumnIndex,ColumnSegmentIndex:s2.ColumnSegmentIndex,ColumnSegment:s2.ColumnSegment)))).OrderBy(s => s.RowIndex).AsEnumerable();
        var x6b = x6.Select(s => (RowIndex:s.RowIndex,Row:s.Row.GroupBy(s2 => s2.ColumnIndex).Select(s2 => (ColumnIndex:s2.Key,Column:s2.Select(s3 => (ColumnSegmentIndex:s3.ColumnSegmentIndex,ColumnSegment:s3.ColumnSegment)).OrderBy(s3 => s3.ColumnSegmentIndex).AsEnumerable())).OrderBy(s2 => s2.ColumnIndex).AsEnumerable()));
        var x5 = x6b.Select(s => (RowIndex:s.RowIndex,Row:s.Row.Select(s2 => (ColumnIndex:s2.ColumnIndex,Column:s2.Column.Select(s3 => s3.ColumnSegment)))));
        var x4 = x5.Select(s => (RowIndex:s.RowIndex,Row:s.Row.Select(s2 => s2.Column))).OrderBy(s => s.RowIndex).AsEnumerable();
        var x3 = x4.Select(s => s.Row);
        var x2 = x3.SelectMany(s => s.Select(s2 => s2.ToArray()).ToArray()).ToArray();
        var x1 = x2.Select(s => s.LetterForPenRows()).ToArray();
        var x = string.Join("",x1);
        return x;
    }
    private static string UnpaledString(this string unpennedString)
    {
        var x = unpennedString;
        var x1 = x.Words().Select(s => string.Join("",s)).ToArray();
        var x2 = x1.ToList().GetRange(0,x1.Length/2+1);
        var x3 = string.Join(" ",x2);
        // var x2 = x.Substring(0,unpennedString.Length/2+1);
        return x3;
    }
    private static string DeepUnpaledString(this string unpaledString)
    {
        var x = unpaledString;
        var x1 = x.Words().Select(s => string.Join("",s)).ToArray();
        
        var x2 = x1.Select(s => s.Length > 2 ? s.Substring(0,s.Length/2+1) : s);
        var x3 = string.Join(" ",x2);

        return x3;
    }
    private static string PennedString(this string linedstr) {
        var x = linedstr;
        var x1 = x.Split("\n");
        var x2 = x1.Select((s) => s.Select(c => c));
        var x3 = x2.Select(s =>  s.SelectMany( c=> Pencodes.PenCodes.Where(pc => pc.Letter == $"{c}").Select((pc) => pc.PenRows.Select(ss => ss))));
        var x4 = x3.Zip(Enumerable.Range(0,int.MaxValue),(row,index)=> (RowIndex:index,Row:row));
        var x5 = x4.Select(s => (RowIndex:s.RowIndex,Row:(s.Row.Zip(Enumerable.Range(0,int.MaxValue),(column,columnIndex) => (ColumnIndex:columnIndex,Column:column)))));
        var x6 = x5.Select(s => (RowIndex:s.RowIndex,Row:(s.Row.Select(s2 => (ColumnIndex:s2.ColumnIndex,Column:s2.Column.Zip(Enumerable.Range(0,int.MaxValue),(columnSeg,columnSegIndex) => (ColumnSegmentIndex:columnSegIndex,ColumnSegment:columnSeg)))))));
        var x7 = x6.SelectMany( s=> s.Row.SelectMany(s2 => s2.Column.Select(s3 => (RowIndex:s.RowIndex,ColumnIndex:s2.ColumnIndex,ColumnSegmentIndex:s3.ColumnSegmentIndex,ColumnSegment:s3.ColumnSegment))));
        var x8 = x7.GroupBy(s => s.RowIndex).Select(s => (RowIndex:s.Key,Row:s.Select(s2 => (ColumnIndex:s2.ColumnIndex,ColumnSegmentIndex:s2.ColumnSegmentIndex,ColumnSegment:s2.ColumnSegment))));
        var x9 = x8.Select(s => (RowIndex:s.RowIndex,Row:s.Row.GroupBy(s2 => s2.ColumnSegmentIndex).Select(s2 => (ColumnSegmentIndex:s2.Key,Columns:s2.Select(s3 => (ColumnIndex:s3.ColumnIndex,ColumnSegment:s3.ColumnSegment)).OrderBy(s3 => s3.ColumnIndex).AsEnumerable()))));
        var x10 = x9.Select(s => (RowIndex:s.RowIndex,Row:s.Row.Select(s2 => (ColumnSegmentIndex:s2.ColumnSegmentIndex,Columns:s2.Columns.OrderBy(s3 => s3.ColumnIndex).Select(s3 => s3.ColumnSegment).ToArray()))));
        var x11 = x10.Select(s => (RowIndex:s.RowIndex,Line:s.Row.OrderBy(s2 => s2.ColumnSegmentIndex).Select(s2 => s2.Columns).ToArray()));
        var x12 = x11.OrderBy(s => s.RowIndex).Select(s => s.Line).ToArray();
        var x13 = x12.SelectMany(s => s).ToArray();
        var x14 = x13.Select(s => string.Join("",s)).ToArray();
        var x15 = string.Join("\n",x14);
        return x15;
    }


    private static bool IsConsonant(this char letter) => Consonants
        .Where((ch) => ch == letter.ToLower())
        .Where((ch) => !ch.IsSpecial() && !ch.IsAntiSpecial())        
        .Any();

    private static IEnumerable<char> All => Enumerable.Range(char.MinValue,char.MaxValue-char.MinValue)
    .Select((chi) => (char)chi);
    
    private static IEnumerable<char> Consonants => All
    .Where(char.IsAsciiLetter)
    .Where(char.IsLower)
    .Where((ch) => !Vowels.Where((v) => v == ch).Any());
    private static IEnumerable<char> UpperVowels => Vowels
    .Select((ch) => ch.ToUpper());
    private static IEnumerable<char> UpperConsonants => Consonants
    .Select((ch) => ch.ToUpper());
    private static bool IsLetter(this char letter) => char.IsAsciiLetter(letter);
    
    private static bool IsUpper(this char letter) => char.IsUpper(letter);

    private static bool IsNumber(this char letter) => char.IsDigit(letter);
    
    private static char ToUpper(this char letter) => char.ToUpperInvariant(letter);
    
    private static char ToLower(this char letter) => char.ToLowerInvariant(letter);

    private static char GetVowel(int index) => index >= 0 ?
        Vowels.Skip(index).First() :
        Vowels.Reverse().Skip(-1*index-1).First();
    private static char GetConsonant(int index) => index >= 0 ?
        Consonants.Skip(index).First() :
        Consonants.Reverse().Skip(-1*index-1).First();
    private static char GetNumberVowel(int index) => index >= 0 ?
        NumberVowels.Skip(index).First() :
        NumberVowels.Reverse().Skip(-1*index-1).First();
    private static char GetNumberConsonant(int index) => index >= 0 ?
        NumberConsonants.Skip(index).First() :
        NumberConsonants.Reverse().Skip(-1*index-1).First();
    private static int GetVowelIndex(this char letter) => Vowels
    .Zip(Enumerable.Range(0,int.MaxValue),(ch,index) => (Character:ch,Index:index))
    .Where((it) => it.Character == letter.ToLower())
    .Select((it) => it.Index)
    .First();
    private static int GetConsonantIndex(this char letter) => Consonants
    .Zip(Enumerable.Range(0,int.MaxValue),(ch,index) => (Character:ch,Index:index))
    .Where((it) => it.Character == letter.ToLower())
    .Select((it) => it.Index)
    .First();
    private static int GetNumberVowelIndex(this char letter) => NumberVowels
    .Zip(Enumerable.Range(0,int.MaxValue),(ch,index) => (Character:ch,Index:index))
    .Where((it) => it.Character == letter)
    .Select((it) => it.Index)
    .First();
    private static int GetNumberConsonantIndex(this char letter) => NumberConsonants
    .Zip(Enumerable.Range(0,int.MaxValue),(ch,index) => (Character:ch,Index:index))
    .Where((it) => it.Character == letter)
    .Select((it) => it.Index)
    .First();
    private static char GetAntiVowel(this char letter) => letter.IsVowel() ?
        letter.IsUpper() ?
            GetVowel(-1*letter.GetVowelIndex()-1).ToUpper()
        :
            GetVowel(-1*letter.GetVowelIndex()-1) 
    : letter;
    private static char GetAntiConsonant(this char letter) => letter.IsConsonant() ?
        letter.IsUpper() ?
            GetConsonant(-1*letter.GetConsonantIndex()-1).ToUpper()
        :
            GetConsonant(-1*letter.GetConsonantIndex()-1)
         : letter;
    private static IEnumerable<char> PencodeLetters => Pencodes.PenCodes
    .Select((pc) => pc.Letter[0]);
    private static char GetAntiLetter(this char letter) => letter
    .GetAntiSpecial()
    .GetAntiVowel()
    .GetAntiConsonant()
    .GetAntiNumberVowel()
    .GetAntiNumberConsonant();
    private static char GetAntiNumberVowel(this char letter) => letter.IsNumberVowel() ?
        GetNumberVowel(-1*letter.GetNumberVowelIndex()-1) : letter;
    private static char GetAntiNumberConsonant(this char letter) => letter.IsNumberConsonant() ?
        GetNumberConsonant(-1*letter.GetNumberConsonantIndex()-1) : letter;
    
    private static IEnumerable<IEnumerable<char>> Words(this IEnumerable<char> str) => 
        str.Aggregate((Words:Enumerable.Empty<string>(),Buffer:""),(state,cur) => {
            var words = state.Words;
            var buffer = state.Buffer;
            if (char.IsWhiteSpace(cur) || !cur.IsPencodeLetter())
            {
                words = words.Append(buffer);
                buffer = "";                
            }
            else {
                buffer = buffer += cur;
            }
            return (Words:words,Buffer:buffer);
        },(fin) => 
        {
            var words = fin.Words;
            var buf = fin.Buffer;
            if (buf.Length > 0)
            {
                words = words.Append(buf);
            }
            return words;
        });
    private static IEnumerable<IEnumerable<char>> Pal2(this IEnumerable<IEnumerable<char>> words) => words
    .Concat(words.Reverse().Skip(1));
    private static IEnumerable<char> Pal1(this IEnumerable<char> str) =>
        str
        .Concat(str.Reverse().Skip(1));
    
    public static IEnumerable<char> ThothCode(this IEnumerable<char> str) => string.Join(" ",string.Join(" ",str.GetAntithesis().Words().Pal2().Select((w) => new string(w.Pal1().ToArray())))).Lines().LinedStr().PennedString();
    public static IEnumerable<char> ThothAnticode(this IEnumerable<char> str,bool padded) 
    {
        var x = string.Join("",str);
        var x1 = x.UnpennedString();
        var xo = padded ? x1.Trim() : x1;
        var x2 = xo.UnpaledString();
        var x3 = x2.DeepUnpaledString();
        var x4 = x3.GetAntithesis();
        return x4;
    }
    
    private static IEnumerable<char> GetAntithesis(this IEnumerable<char> str) => str
    .Select((ch) => ch.GetAntiLetter());
    public static string ThothString(this string str) => 
    string.Join("",str.ThothCode().ToArray());
    public static string UnthothString(this string str,bool padded=false) => 
    string.Join("",str.ThothAnticode(padded).ToArray());
    
}
