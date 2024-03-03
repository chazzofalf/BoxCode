// See https://aka.ms/new-console-template for more information
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BoxCode;
using SkiaSharp;


internal class Program
{
    private static void GetHelp()
    {
        var helptext= @"
BoxCode OPERATION input output
OPERATION
-e Encode a text into a image.
-d Decode a text from a image.
";
            System.Console.WriteLine(helptext);
    }
    private static void Main(string[] args)
    {
var td = "C:\\Temp\\Fonts";
        
        if (args.Length == 0)
        {
            GetHelp();
            return;
        }
        else if (args.Length == 1)
        {
            if (args[0] == "--demo")
            {
                Test.Execute();
                return;
            }                        
        }
        else if (args.Length == 2)
        {
            if (args[0] == "-ef")
            {
                Directory.CreateDirectory(args[1]);
                Pencodes.PenCodes.Aggregate((object)null,(nothing,cur) => {
                    char letter = cur.Letter[0];
                    try
                    {
                        File.WriteAllBytes($"{args[1]}\\{System.Convert.ToHexString(Encoding.UTF8.GetBytes($"{letter}"))}_{letter}.png",cur.PenRowsBmp.Encode(SKEncodedImageFormat.Png,100).ToArray());
                    }
                    catch
                    {
                        File.WriteAllBytes($"{args[1]}\\{System.Convert.ToHexString(Encoding.UTF8.GetBytes($"{letter}"))}.png",cur.PenRowsBmp.Encode(SKEncodedImageFormat.Png,100).ToArray());
                    }
                    
                    return null;
                });
                return;
            }
        }
        else if (args.Length == 3)
        {
            if (args[0] == "-e")
            {                
                File.WriteAllBytes(args[2],File.ReadAllText(args[1]).CreateBitmap().Encode(SKEncodedImageFormat.Png,100).ToArray());
                return;
            }
            else if (args[0] == "-e1")
            {
                File.WriteAllBytes(args[2],File.ReadAllText(args[1]).CreateBitmap(singleLine:true).Encode(SKEncodedImageFormat.Png,100).ToArray());
                return;
            }
            else if (args[0] == "-ve1")
            {
                File.WriteAllBytes(args[2],File.ReadAllText(args[1]).TovaBitmap(singleLine:true).Encode(SKEncodedImageFormat.Png,100).ToArray());
                return;
            }
            else if (args[0] == "-ve")
            {
                File.WriteAllBytes(args[2],File.ReadAllText(args[1]).TovaBitmap(singleLine:false).Encode(SKEncodedImageFormat.Png,100).ToArray());
                return;
            }
            else if (args[0] == "-d")
            {
                File.WriteAllText(args[2],SKBitmap.Decode(File.ReadAllBytes(args[1])).FromBitmap());
                return;
            }
            
        }
        GetHelp();
        
    }
}