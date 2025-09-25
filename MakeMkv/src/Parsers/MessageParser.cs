using System.Text.RegularExpressions;

namespace DiscRipper.MakeMkv;

internal class MessageParser : Parser<Message>
{
    public override string Pattern => """
        MSG:(\d+),(\d+),(\d+),"([^"]*)"
        """;

    public override Message CreateValueFromMatch(Match match)
    {
        //     1    2        3  4                                       -                                -            -
        // MSG:3307,16777216,2,"File 00002.mpls was added as title #0","File %1 was added as title #%2","00002.mpls","0"

        Message message = new()
        {
            Code = int.Parse(match.Groups[1].Value),
            Flags = int.Parse(match.Groups[2].Value),
            Text = match.Groups[4].Value
        };

        return message;
    }
}
