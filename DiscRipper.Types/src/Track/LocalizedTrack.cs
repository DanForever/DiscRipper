namespace DiscRipper.Types;

public abstract class LocalizedTrack : Track
{
    public required string LanguageCode { get; set; }
    public required string Language { get; set; }
}
