using System.Text.RegularExpressions;

namespace DiscRipper.MakeMkv;

public class TitleInfoParser : Parser<TitleInfo>
{
    public override string Pattern => """
        TINFO:(\d+),(\d+),(\d+),"([^"]*)"
        """;

    public override TitleInfo CreateValueFromMatch(Match match)
    {
        // The third group, for the most part, appears to be 0, and the documentation does appear to acknowledge it's existence at all
        TitleInfo titleInfo = new()
        {
            Id = int.Parse(match.Groups[1].Value),
            Code = int.Parse(match.Groups[2].Value),
            Value = match.Groups[4].Value
        };

        return titleInfo;
    }
}
