namespace DiscRipper.ViewModel;

internal class Submission : ViewModel
{
    public required TheDiscDb.Submission Model { get; init; }

    public string? TMDB
    {   get => Model.TMDB; 
        set => ChangeProperty(Model, value);
    }

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
    public TheDiscDb.RegionCode RegionCode { get; set; }
    public string? Locale { get; set; }

    public required IEnumerable<SubmissionTitle> Titles { get; init; }
}
