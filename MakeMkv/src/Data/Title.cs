using System.Diagnostics;

namespace DiscRipper.MakeMkv;

[DebuggerDisplay("{Duration} ('{Filename,nq}')")]
public class Title
{
    public int Index { get; set; }

    public string DiscName { get; set; } = string.Empty;
    public string SourceFilename { get; set; } = string.Empty;

    public string Duration { get; set; } = string.Empty;

    public int DurationInSeconds { get; set; }

    public int ChaptersCount { get; set; }
    public string Size { get; set; } = string.Empty;
    public int SegmentCount { get; set; }
    public string SegmentMap { get; set; } = string.Empty;

    public string Filename { get; set; } = string.Empty;
}
