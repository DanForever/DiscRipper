using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using DiscRipper.Types;

namespace DiscRipper.MakeMkv;

public class TitleEngine
{
    private class TitleTracks
    {
        public ConcurrentDictionary<int, Track> Tracks { get; private init; } = new();
    }

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

        await Parallel.ForEachAsync(log.TitleInfo, (titleInfo, _) =>
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

            case TitleInfoType.FileSizeBytes:
                title.SizeInBytes = long.Parse(titleInfo.Value);
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

        await Parallel.ForEachAsync<Title>(Titles, (title, _) =>
        {
            var trackInfoForTitle = log.TrackInfo
                .Where(trackInfo => trackInfo.TitleId == title.Index)
                .Select(trackInfo => trackInfo.Id)
                .Distinct();

            int largestId = trackInfoForTitle.Max();

            title.Tracks = new Track[largestId + 1];

            for(int i = 0; i < title.Tracks.Length; ++i)
            {
                title.Tracks[i] = CreateTrack(log, title.Index, i);
            }

            return ValueTask.CompletedTask;
        });
    }

    private static Track CreateTrack(Log log, int titleId, int trackId)
    {
        IEnumerable<TrackInfo> trackInfo = log.TrackInfo.Where(trackInfo => trackInfo.TitleId == titleId && trackInfo.Id == trackId);

        string? type = null;
        string? name = null;

        string? resolution = null;
        string? aspectRatio = null;

        string? languageCode = null;
        string? language = null;

        string? audioType = null;

        foreach (var item in trackInfo)
        {
            switch(item.Type)
            {
            case TrackInfoType.Type:
                type = item.Value;
                break;

            case TrackInfoType.Name:
                name = item.Value;
                break;

            case TrackInfoType.Resolution:
                resolution = item.Value;
                break;

            case TrackInfoType.AspectRatio:
                aspectRatio = item.Value;
                break;

            case TrackInfoType.LanguageCode:
                languageCode = item.Value;
                break;

            case TrackInfoType.Language:
                language = item.Value;
                break;

            case TrackInfoType.AudioType:
                audioType = item.Value;
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new InvalidOperationException($"Track type not found for title {titleId} track {trackId}");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException($"Track name not found for title {titleId} track {trackId}");
        }

        switch (type)
        {
        case "Video":
            if (string.IsNullOrWhiteSpace(resolution))
            {
                throw new InvalidOperationException($"Video track resolution not found for title {titleId} track {trackId}");
            }

            if (string.IsNullOrWhiteSpace(aspectRatio))
            {
                throw new InvalidOperationException($"Video track aspect ratio not found for title {titleId} track {trackId}");
            }

            return new VideoTrack()
            {
                Index = trackId,
                Type = type,
                Name = name,
                Resolution = resolution,
                AspectRatio = aspectRatio
            };

        case "Audio":

            if (string.IsNullOrWhiteSpace(languageCode))
            {
                throw new InvalidOperationException($"Audio track language code not found for title {titleId} track {trackId}");
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                throw new InvalidOperationException($"Audio track language not found for title {titleId} track {trackId}");
            }

            if (string.IsNullOrWhiteSpace(audioType))
            {
                throw new InvalidOperationException($"Audio track audiotype not found for title {titleId} track {trackId}");
            }

            return new AudioTrack()
            {
                Index = trackId,
                Type = type,
                Name = name,
                LanguageCode = languageCode,
                Language = language,
                AudioType = audioType
            };

        case "Subtitles":
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                throw new InvalidOperationException($"Audio track language code not found for title {titleId} track {trackId}");
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                throw new InvalidOperationException($"Audio track language not found for title {titleId} track {trackId}");
            }

            return new SubtitleTrack()
            {
                Index = trackId,
                Type = type,
                Name = name,
                LanguageCode = languageCode,
                Language = language
            };
        }

        throw new InvalidOperationException($"Unknown title type {titleId} track {trackId}");
    }
}
