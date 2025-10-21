using System.Xml.Serialization;

namespace DiscRipper.Types;

[XmlInclude(typeof(VideoTrack))]
[XmlInclude(typeof(SubtitleTrack))]
[XmlInclude(typeof(AudioTrack))]
public abstract class Track
{
    public int Index { get; set; }
    public required string Name { get; set; }
    public required string Type { get; init; }
}
