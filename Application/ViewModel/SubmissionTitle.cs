using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DiscRipper.ViewModel;

internal class SubmissionTitle : ViewModel
{
    public required TheDiscDb.Title Model { get; init; }

    public string Name
    {
        get => Model.Name;
        set => ChangeProperty(value);
    }

    public string SourceFileName
    {
        get => Model.SourceFileName;
        set => ChangeProperty(value);
    }

    public string Duration
    {
        get => Model.Duration;
        set => ChangeProperty(value);
    }

    public int ChaptersCount
    {
        get => Model.ChaptersCount;
        set => ChangeProperty(value);
    }

    public string Size
    {
        get => Model.Size;
        set => ChangeProperty(value);
    }

    public int SegmentCount
    {
        get => Model.SegmentCount;
        set => ChangeProperty(value);
    }

    public string SegmentMap
    {
        get => Model.SegmentMap;
        set => ChangeProperty(value);
    }

    public TheDiscDb.TitleType Type
    {
        get => Model.Type;
        set => ChangeProperty(value);
    }

    public int? Season
    {
        get => Model.Season;
        set => ChangeProperty(value);
    }

    public int? Episode
    {
        get => Model.Episode;
        set => ChangeProperty(value);
    }

    public string Filename
    {
        get => Model.Filename;
        set => ChangeProperty(value);
    }

    public string Format => Model.Format;

    protected void ChangeProperty<T>(T value, [CallerMemberName] string propertyName = "")
    {
        base.ChangeProperty(Model, value, propertyName);
        AnnouncePropertyChanged(nameof(Format));
    }
}
