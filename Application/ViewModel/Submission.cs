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

    public string? DiscFormat
    {
        get => Model.DiscFormat;
        set => ChangeProperty(Model, value);
    }

    public string? ReleaseSlug
    {
        get => Model.ReleaseSlug;
        set => ChangeProperty(Model, value);
    }

    public string? UPC
    {
        get => Model.UPC;
        set => ChangeProperty(Model, value);
    }

    public string? ASIN
    {
        get => Model.ASIN;
        set => ChangeProperty(Model, value);
    }

    public DateTime? PublicationDate
    {
        get => Model.PublicationDate;
        set => ChangeProperty(Model, value);
    }

    public string? FrontCoverUrl
    {
        get => Model.FrontCoverUrl;
        set => ChangeProperty(Model, value);
    }

    public string? BackCoverUrl
    {
        get => Model.BackCoverUrl;
        set => ChangeProperty(Model, value);
    }

    public string? EditionName
    {
        get => Model.EditionName;
        set => ChangeProperty(Model, value);
    }

    public string? DiscTitle
    {
        get => Model.DiscTitle;
        set => ChangeProperty(Model, value);
    }

    public string? DiscSlug
    {
        get => Model.DiscSlug;
        set => ChangeProperty(Model, value);
    }

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
