namespace DiscRipper.ViewModel;

internal class Submission : ViewModel
{
    public required TheDiscDb.Submission Model { get; init; }

    public string? TMDB
    {
        get => Model.TMDB; 
        set => ChangeProperty(Model, value);
    }

    public string? MediaType
    {
        get => Model.MediaType;
        set => ChangeProperty(Model, value);
    }

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

    public TheDiscDb.RegionCode RegionCode
    {
        get => Model.RegionCode;
        set => ChangeProperty(Model, value);
    }

    public string? Locale
    {
        get => Model.Locale;
        set => ChangeProperty(Model, value);
    }

    public required IEnumerable<SubmissionTitle> Titles { get; init; }
}
