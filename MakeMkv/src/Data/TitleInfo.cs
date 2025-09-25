using System.Diagnostics;

namespace DiscRipper.MakeMkv;

public enum TitleType
{
    Unknown = -1,

    DiscTitle = 2,
    NumberOfChapters = 8,
    Length = 9,
    FileSizeGb = 10,
    FileSizeBytes = 11,
    Filename = 27,
    AudioShortCode = 28,
    AudioLongCode = 29,
}

[DebuggerDisplay("{Code} => {Value,nq}")]
public class TitleInfo
{
    public int Id { get; set; }
    public int Code { get; set; }
    public required string Value { get; set; }
    public TitleType Type => ConvertCodeToTitleType(Code);

    private static TitleType ConvertCodeToTitleType(int code)
    {
        if(Enum.IsDefined(typeof(TitleType),code))
        {
            return (TitleType)code;
        }

        return TitleType.Unknown;
    }
}
