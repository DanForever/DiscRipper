using System.Text.RegularExpressions;

namespace DiscRipper.MakeMkv;

public class DiscInfoParser : Parser<DiscInfo>
{
    public override string Pattern => """
        CINFO:(\d+),(\d+),"([^"]*)"
        """;

    public override DiscInfo CreateValueFromMatch(Match match)
    {
        DiscInfo disc = new()
        {
            Id = int.Parse(match.Groups[1].Value),
            Code = int.Parse(match.Groups[2].Value),
            Value = match.Groups[3].Value
        };

        return disc;
    }
}
