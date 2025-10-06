namespace DiscRipper.Types;

public abstract class Track
{
    public int Index { get; set; }
    public required string Name { get; set; }
    public required string Type { get; init; }
}
