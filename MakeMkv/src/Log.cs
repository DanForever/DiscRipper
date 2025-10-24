using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscRipper.MakeMkv;

public class RawLine
{
    public int Index { get; set; }
    public required string Line { get; set; }
}

public class Log
{
    private readonly TitleCount _titleCount = new();
    private readonly ConcurrentBag<RawLine> _raw = [];
    private readonly ConcurrentBag<Drive> _drives = [];
    private readonly ConcurrentBag<Message> _messages = [];
    private readonly ConcurrentBag<DiscInfo> _discInfo = [];
    private readonly ConcurrentBag<TitleInfo> _titleInfo = [];
    private readonly ConcurrentBag<TrackInfo> _trackInfo = [];

    public required SynchronizationContext SynchronizationContext { get; set; }

    public TitleCount TitleCount => _titleCount;
    public ConcurrentBag<RawLine> Raw => _raw;
    public ConcurrentBag<Drive> Drives => _drives;
    public ConcurrentBag<TitleInfo> TitleInfo => _titleInfo;
    public ConcurrentBag<TrackInfo> TrackInfo  => _trackInfo;

    private IParser[] Parsers =>
    [
        new DriveParser() { Values = _drives },
        new MessageParser() { Values = _messages },
        new DiscInfoParser() { Values = _discInfo },
        new TitleInfoParser() { Values = _titleInfo },
        new TrackInfoParser() { Values = _trackInfo },
        new TitleCountParser() { TitleCount = _titleCount }
    ];

    #region Events

    public delegate void OutputHandler(string output);
    public event OutputHandler? StandardOutputReceived;
    public event OutputHandler? StandardErrorReceived;

    #endregion Events

    public void Parse(int index, string line)
    {
        Raw.Add(new RawLine() { Index = index, Line = line });
        SynchronizationContext.Post( _ => StandardOutputReceived?.Invoke(line), null);

        foreach (var parser in Parsers)
        {
            if (parser.Parse(line))
                return;
        }

        Console.WriteLine($"No match: {line}");
    }

    public string ExportRawLog(bool anonymize = false)
    {
        StringBuilder builder = new();

        // todo: unify with the pattern in drive parser
        const string Pattern = """
            DRV:(\d+),(\d+),(\d+),(\d+),"([^"]*)","([^"]*)","([^"]*)"
            """;

        const string Replacement = """
            DRV:$1,$2,$3,$4,"***","$6","***"
            """;

        var lines = _raw.OrderBy(line => line.Index).Select(line => line.Line);

        foreach (string line in lines)
        {
            if (anonymize)
                builder.AppendLine(Regex.Replace(line, Pattern, Replacement));
            else
                builder.AppendLine(line);
        }

        return builder.ToString();
    }
}
