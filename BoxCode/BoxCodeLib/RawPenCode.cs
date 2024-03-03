namespace BoxCodeLib;
using Newtonsoft.Json.Serialization;

internal class RawPenCode
{
    public string? Letter {get; set; }
    public string[]? PenRows {get; set; }
    public bool? IsSpecial {get; set;}
    public bool? IsStart {get; set;}
    public bool? IsReverseStart {get; set;}
    public bool? IsEnd {get; set;}
    public bool? IsUsed {get; set;}

}
