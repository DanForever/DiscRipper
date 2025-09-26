namespace DiscRipper.TheDiscDb;

public enum RegionCode
{
    RegionFree = 0,

    // North America, South America, Japan, South Korea
    RegionA1,

    // Europe, Africa, Middle East, Australia, New Zealand
    RegionB2,

    // China, Russia, India
    RegionC3,
}

public class Submission
{
    public string? TMDB { get; set; }
    public string? MediaType { get; set; }
    public string? DiscFormat { get; set; }
    public string? ReleaseSlug { get; set; }
    public string? UPC { get; set; }
    public string? ASIN { get; set; }
    public DateTime? PublicationDate { get; set; }
    public string? FrontCoverUrl { get; set; }
    public string? BackCoverUrl { get; set; }
    public string? EditionName { get; set; }
    public string? DiscTitle { get; set; }
    public string? DiscSlug { get; set; }
    public RegionCode RegionCode { get; set; }
    public string? Locale { get; set; }

    public required IEnumerable<Title> Titles { get; init; }
}
