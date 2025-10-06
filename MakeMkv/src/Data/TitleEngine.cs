using System.Diagnostics.CodeAnalysis;

using DiscRipper.Types;

namespace DiscRipper.MakeMkv;

public class TitleEngine
{
    public Title[]? Titles { get; private set; }

    public Title? LongestTitle => Titles?.MaxBy(title => title.DurationInSeconds);

    [MemberNotNull(nameof(Titles))]
    public async Task Read(Log log)
    {
        Titles = new Title[log.TitleCount.Count];
        for(int i = 0; i < Titles.Length; ++i)
        {
            Titles[i] = new Title() { Index = i };
        }

        await Parallel.ForEachAsync<TitleInfo>(log.TitleInfo, (titleInfo, _) =>
        {
            Title title = Titles[titleInfo.Id];

            switch (titleInfo.Type)
            {
            case TitleInfoType.DiscName:
                title.DiscName = titleInfo.Value;
                break;

            case TitleInfoType.SourceFilename:
                title.SourceFilename = titleInfo.Value;
                break;

            case TitleInfoType.Length:
                title.Duration = titleInfo.Value;
                title.DurationInSeconds = (int)TimeSpan.Parse(titleInfo.Value).TotalSeconds;
                break;

            case TitleInfoType.NumberOfChapters:
                title.ChaptersCount = int.Parse(titleInfo.Value);
                break;

            case TitleInfoType.FileSizeGb:
                title.Size = titleInfo.Value;
                break;

            case TitleInfoType.SegmentCount:
                title.SegmentCount = int.Parse(titleInfo.Value);
                break;

            case TitleInfoType.SegmentMap:
                title.SegmentMap = titleInfo.Value;
                break;

            case TitleInfoType.Filename:
                title.Filename = titleInfo.Value;
                break;
            }

            return ValueTask.CompletedTask;
        });
    }
}
