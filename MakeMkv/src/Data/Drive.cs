using System.Diagnostics;

namespace DiscRipper.MakeMkv;

public enum DriveStatus
{
    Empty = 0,
    Open = 1,
    Closed = 2,
    Loading = 3,
    NotAttached = 256
}

public enum DiscType
{
    CD = 0,
    DVD = 1,
    BD = 12
}

[DebuggerDisplay("{DriveName}")]
public class Drive
{
    public int Index { get; set; }

    public DriveStatus DriveStatus { get; set; }

    public DiscType DiscType { get; set; }

    public required string DriveName { get; set; }

    public required string MediaTitle { get; set; }

    public required string DrivePath { get; set; }
}
