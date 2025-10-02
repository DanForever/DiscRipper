namespace DiscRipper;

public static class MakeMkvToTheDiscDb
{
    public static TheDiscDb.Title Convert(MakeMkv.Title source)
    {
        TheDiscDb.Title title = new()
        {
            Name = source.DiscName,
            SourceFileName = source.SourceFilename,
            Duration = source.Duration,
            ChaptersCount = source.ChaptersCount,
            Size = source.Size,
            SegmentCount = source.SegmentCount,
            SegmentMap = source.SegmentMap,

            Type = TheDiscDb.TitleType.Ignore,
            Season = 0,
            Episode = 0,

            Filename = source.Filename,
        };

        return title;
    }

    public static IEnumerable<TheDiscDb.Title> Convert(IEnumerable< MakeMkv.Title> source)
    {
        List<TheDiscDb.Title> outTitles = [];

        foreach (var title in source)
        {
            outTitles.Add(Convert(title));
        }

        return outTitles;
    }
}
