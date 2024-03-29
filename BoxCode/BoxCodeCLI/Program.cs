﻿// See https://aka.ms/new-console-template for more information
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BoxCodeLib;
using SkiaSharp;


internal class Program
{
    private static void GetHelp()
    {
        var helptext= @"
BoxCode OPERATION input output
OPERATION
-e Encode a text into a image. Squared.
-e1 Encode a text into a image. Single Line.
-d Decode a text from a image.
";
            System.Console.WriteLine(helptext);
    }
    private static void Main(string[] args)
    {

        // var rect = new SKRect(0,0,4,4);
        // System.Console.WriteLine(rect.Size);
        
        if (args.Length == 0)
        {
            GetHelp();
            return;
        }        
        else if (args.Length == 3)
        {
            
            if (args[0] == "-e1")
            {
                File.WriteAllBytes(args[2], BoxCodeLib.LibraryUtil.ConvertToBitmap(File.ReadAllText(args[1]),singleLine:true).Encode(SKEncodedImageFormat.Png,100).ToArray());
                return;
            }
            else if (args[0] == "-e")            
            {
                File.WriteAllBytes(args[2], BoxCodeLib.LibraryUtil.ConvertToBitmap(File.ReadAllText(args[1]),singleLine:false).Encode(SKEncodedImageFormat.Png,100).ToArray());                
                return;
            }
            else if (args[0] == "-d")
            {
                File.WriteAllText(args[2],BoxCodeLib.LibraryUtil.ConvertFromBitmap(SKBitmap.Decode(File.ReadAllBytes(args[1]))));                
                return;
            }
            
            
        }
        GetHelp();
        
    }
}