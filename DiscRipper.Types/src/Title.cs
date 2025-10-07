namespace DiscRipper.Types;

public enum TitleType
{
    Ignore,

    MainMovie,
    DeletedScene,
    Trailer,
    Extra,
    Episode,
}

[System.Diagnostics.DebuggerDisplay("{Duration} ('{Filename,nq}')")]
public class Title
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DiscName { get; set; } = string.Empty;
    public string SourceFilename { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public int DurationInSeconds { get; set; }
    public int ChaptersCount { get; set; } = 0;
    public string Size { get; set; } = string.Empty;
    public int SegmentCount { get; set; } = 0;
    public string SegmentMap { get; set; } = string.Empty;

    public TitleType Type { get; set; } = TitleType.Ignore;
    public int? Season { get; set; }
    public int? Episode { get; set; }

    public string Filename { get; set; } = string.Empty;

    public Track[] Tracks { get; set; } = [];

    public string Format => $"""
        Name: {Name}
        Source file name: {SourceFilename}
        Duration: {Duration}
        Chapters count: {ChaptersCount}
        Size: {Size}
        Segment count: {SegmentCount}
        Segment map: {SegmentMap}
        Type: {Type}

        """ +

        (Type == TitleType.Episode? $"""
        Season {Season}
        Episode {Episode}

        """ : string.Empty) +

        $"""
        File name: {Filename}
        """;
}
