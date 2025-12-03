namespace DiscRipper.Types;

public record Release
{
	public required string Slug { get; init; }
	public required string Path { get; init; }
}

public record Media
{
	public required string Title { get; init; }
	public required IEnumerable<Release> Releases { get; init; }
	public required string Path { get; init; }
}
