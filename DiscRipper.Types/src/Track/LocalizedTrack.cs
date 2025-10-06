namespace DiscRipper.Types;

public abstract class LocalizedTrack : Track
{
    public required string Resolution { get; set; }
    public required string AspectRatio { get; set; }
}
