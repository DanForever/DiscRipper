using System.Xml.Serialization;

namespace DiscRipper.Types.Hash;

// Copied from ImportBuddy
public record File
{
	public int Index { get; set; }
	public string? Name { get; set; }
	public DateTime CreationTime { get; set; }
	public long Size { get; set; }
}

[XmlType("DiscHash")]
public class Disc
{
	public required string Hash { get; set; }

	public required List<File> Files { get; set; }
}
