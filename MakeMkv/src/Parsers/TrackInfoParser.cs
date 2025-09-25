using System.Text.RegularExpressions;

namespace DiscRipper.MakeMkv;

enum TrackInfoType
{
    // Shared
    Type = 1, // Audio / Video
    Name = 2,
    Codec = 7,

    // Video
    Resolution = 19,
    AspectRatio = 20,
    FrameRate = 21,

    // Audio
    Language = 4,
    Channels = 14,
    ChannelLayout = 40,
    SampleRate = 17,
    BitsPerSample = 18,
}

public class TrackInfoParser : Parser<TrackInfo>
{
    public override string Pattern => """
            SINFO:(\d+),(\d+),(\d+),(\d+),"([^"]*)"
            """;

    public override TrackInfo CreateValueFromMatch(Match match)
    {
        TrackInfo trackInfo = new()
        {
            TitleId = int.Parse(match.Groups[1].Value),
            Id = int.Parse(match.Groups[2].Value),
            Code = int.Parse(match.Groups[3].Value),
            Unknown = int.Parse(match.Groups[3].Value),
            Value = match.Groups[5].Value
        };

        return trackInfo;
    }
}
