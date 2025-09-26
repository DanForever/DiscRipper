namespace DiscRipper.TheDiscDb;

public class Title
{
    public string Name { get; set; } = string.Empty;
    public string SourceFileName { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public int ChaptersCount { get; set; } = 0;
    public string Size { get; set; } = string.Empty;
    public int SegmentCount { get; set; } = 0;
    public string SegmentMap { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
    public int? Season { get; set; }
    public int? Episode { get; set; }

    public string Filename { get; set; } = string.Empty;

    public string Format => $"""
        Name: {Name}
        Source file name: {SourceFileName}
        Duration: {Duration}
        Chapters count: {ChaptersCount}
        Size: {Size}
        Segment count: {SegmentCount}
        Segment map: {SegmentMap}
        File name: {Filename}
        """;
}
