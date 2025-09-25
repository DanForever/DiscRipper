namespace DiscRipper.MakeMkv;

public class TitleCountParser : Parser
{
    public required TitleCount TitleCount { get; init; }

    public override string Pattern => """TCOUNT:(\d+)""";

    public override bool Parse(string line)
    {
        var match = Regex.Match(line);
        if (match.Success)
        {
            TitleCount.Count = int.Parse(match.Groups[1].Value);
            return true;
        }

        return false;
    }
}
