using System.Text.Json;

using DiscRipper.TheDiscDb.ImportBuddy;

using Fantastic.FileSystem;

using OGIb = global::ImportBuddy;
using OGTddb = global::TheDiscDb;

namespace DiscRipper.TheDiscDb.Submit;

public class WriteDiscJson : IStep
{
    public async Task Run(SubmissionContext context)
    {
        if(context.ReleaseFolder is null)
            throw new ArgumentException("Release folder is not set.");

        var fileSystem = context.InitializationData.FileSystem;

        string discJsonFilePath = fileSystem.Path.Combine(context.ReleaseFolder, $"{context.DiscName}.json");
        if (!await fileSystem.File.Exists(discJsonFilePath))
        {
            var discJsonFile = new OGTddb.InputModels.Disc
            {
                Index = context.DiscIndex,
                Slug = context.Disc.DiscSlug,
                Name = context.Disc.DiscTitle,
                Format = context.Disc.DiscFormat,
                ContentHash = context.DiscHash?.Hash
            };

            foreach(var title in context.Disc.Titles)
            {
                OGTddb.InputModels.Title tddbTitle = new()
                {
                    Index = title.Index,
                    SourceFile = title.SourceFilename,
                    SegmentMap = title.SegmentMap,
                    Duration = title.Duration,
                    Size = title.SizeInBytes,
                    DisplaySize = title.Size,
                    Comment = title.Filename,
                };

                if(title.Type != DiscRipper.Types.TitleType.Ignore)
                {
                    tddbTitle.Item = new()
                    {
                        Title = title.Name,
                        Type = title.Type.ToString(),
                        Season = title.Season?.ToString(),
                        Episode = title.Episode?.ToString()
                    };
                }

                foreach(var track in title.Tracks)
                {
                    switch(track)
                    {
                    case DiscRipper.Types.VideoTrack videoTrack:
                        tddbTitle.Tracks.Add(videoTrack.CreateTddbExportType());
                        break;

                    case DiscRipper.Types.AudioTrack audioTrack:
                        tddbTitle.Tracks.Add(audioTrack.CreateTddbExportType());
                        break;

                    case DiscRipper.Types.SubtitleTrack subtitleTrack:
                        tddbTitle.Tracks.Add(subtitleTrack.CreateTddbExportType());
                        break;
                    }
                }

                discJsonFile.Titles.Add(tddbTitle);
            }

            await fileSystem.File.WriteAllText(discJsonFilePath, JsonSerializer.Serialize(discJsonFile, OGIb.JsonHelper.JsonOptions));
        }
    }
}
