using System.Xml.Serialization;

namespace DiscRipper.TheDiscDb.Submission;

[XmlType("SubmissionDisc")]
public class Disc
{
	public string? DiscFormat { get; set; }
	public string? DiscTitle { get; set; }
	public string? DiscSlug { get; set; }
	public required List<DiscRipper.Types.Title> Titles { get; init; }
}

[XmlType("SubmissionRelease")]
public class Release
{
    public string? TMDB { get; set; }
    public string? MediaType { get; set; }
    public string? ReleaseSlug { get; set; }
    public string? UPC { get; set; }
    public string? ASIN { get; set; }
    public DateTime? PublicationDate { get; set; }
    public string? FrontCoverUrl { get; set; }
    public string? BackCoverUrl { get; set; }
    public string? EditionName { get; set; }
    public RegionCode RegionCode { get; set; }
    public string? Locale { get; set; }

}
