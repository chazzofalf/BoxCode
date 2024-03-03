namespace BoxCode;

public class PenCode
{
    public string Letter {get;  }
    public string[] PenRows {get;  }
    internal PenCode(RawPenCode rpc) {
        if (rpc.Letter != null && rpc.PenRows != null)
        {
            Letter= rpc.Letter;
            PenRows = rpc.PenRows;
        }
        else
        {
            throw new ArgumentException();
        }
    }
    internal PenCode(string letter,string[] penRows)
    {
        Letter = letter;
        PenRows = penRows;
    }
}
