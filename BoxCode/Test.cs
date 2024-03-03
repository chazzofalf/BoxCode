using SkiaSharp;

namespace BoxCode;

public class Test
{
    public static void Execute()
    {
        var msg =
            "BoxCode - A wild take on the pig pen cipher.";
        
        var msgThoth = msg.ThothString();
        var msga = msgThoth.UnthothString();
        var msgThothbmp = msg.CreateBitmap();

        Console.WriteLine(msg);
        Console.WriteLine(msgThoth);
        System.IO.File.WriteAllText("C:\\Temp\\output.txt",msgThoth);
        var file = File.Open("C:\\Temp\\output.png",FileMode.Create,FileAccess.ReadWrite);
        msgThothbmp.Encode(SKEncodedImageFormat.Png,100).SaveTo(file);
        
        file.Flush();
        file.Close();
        System.Console.WriteLine($"Decoded from Image: {SKBitmap.Decode("C:\\Temp\\output.png").FromBitmap()}");

        // var exhaustMsg = string.Join("",Pencodes.PenCodes
        // .Select((pc) => pc.Letter));
        // exhaustMsg = string.Join("",Enumerable.Repeat(exhaustMsg,2));
        // var exhaustMsgThoth = string.Join("",exhaustMsg.ThothString());
        // var exhaustMsgThothBmp = exhaustMsg.CreateBitmap();
        // var exhaustMsgAthoth = exhaustMsgThoth.UnthothString();
        // Console.WriteLine(exhaustMsg);
        // Console.WriteLine(exhaustMsgThoth);
        // Console.WriteLine(exhaustMsgAthoth);
        // System.IO.File.WriteAllText("C:\\Temp\\exhaust.txt",exhaustMsgThoth);
        // System.IO.File.WriteAllText("C:\\Temp\\unexhaust.txt",exhaustMsgAthoth);
        // file = File.Open("C:\\Temp\\exhaust.png",FileMode.Create,FileAccess.ReadWrite);
        // exhaustMsgThothBmp.Encode(SKEncodedImageFormat.Png,100).SaveTo(file);
        // file.Flush();
        // file.Close();
    }
}
