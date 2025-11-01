namespace DiscRipper.Types.Hash;

// Copied from ImportBuddy
public record File
{
	public int Index { get; set; }
	public string? Name { get; set; }
	public DateTime CreationTime { get; set; }
	public long Size { get; set; }
}

public class Disc
{
	public required string Hash { get; set; }

	public required List<File> Files { get; set; }
}
