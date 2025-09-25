using System.Diagnostics.CodeAnalysis;

namespace DiscRipper.MakeMkv;

public class TitleEngine
{
    public Title[]? Titles { get; private set; }

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

            switch(titleInfo.Type)
            {
            case TitleType.Length:
                title.Duration = titleInfo.Value;
                title.DurationInSeconds = (int)TimeSpan.Parse(titleInfo.Value).TotalSeconds;
                break;

            case TitleType.Filename:
                title.Filename = titleInfo.Value;
                break;
            }

            return ValueTask.CompletedTask;
        });
    }
}
