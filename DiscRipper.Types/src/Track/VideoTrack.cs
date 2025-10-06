namespace DiscRipper.Types;

public class VideoTrack : Track
{
    public required string Resolution { get; set; }
    public required string AspectRatio { get; set; }

    public VideoTrack() => Type = "Video";
}
