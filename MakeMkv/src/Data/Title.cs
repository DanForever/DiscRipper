using System.Diagnostics;

namespace DiscRipper.MakeMkv;

[DebuggerDisplay("{Duration} ('{Filename,nq}')")]
public class Title
{
    public int Index { get; set; }

    public string Duration { get; set; } = "";

    public int DurationInSeconds { get; set; }

    public string Filename { get; set; } = "";
}
