using System.Diagnostics;

namespace DiscRipper.MakeMkv;

public enum TitleInfoType
{
    Unknown = -1,

    DiscName = 2,
    NumberOfChapters = 8,
    Length = 9,
    FileSizeGb = 10,
    FileSizeBytes = 11,
    SourceFilename = 16,
    SegmentCount = 25,
    SegmentMap = 26,
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
    public TitleInfoType Type => ConvertCodeToTitleType(Code);

    private static TitleInfoType ConvertCodeToTitleType(int code)
    {
        if(Enum.IsDefined(typeof(TitleInfoType),code))
        {
            return (TitleInfoType)code;
        }

        return TitleInfoType.Unknown;
    }
}
