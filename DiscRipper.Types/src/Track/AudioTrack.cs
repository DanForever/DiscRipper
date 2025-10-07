namespace DiscRipper.Types;

public class AudioTrack : LocalizedTrack
{
    public AudioTrack() => Type = "Audio";

    public required string AudioType { get; set; }
}
