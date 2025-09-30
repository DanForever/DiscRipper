using System.Text.Json;

using Fantastic.FileSystem;

using OGIb = global::ImportBuddy;

namespace DiscRipper.TheDiscDb.Submit;

public class WriteTmdb : IStep
{
    public async Task Run(SubmissionContext context)
    {
        if (context.ImportItem == null)
        {
            throw new ArgumentException("ImportItem must be set in context before writing TMDB data.");
        }

        if(context.BasePath == null)
        {
            throw new ArgumentException("BasePath must be set in context before writing TMDB data.");
        }

        var fileSystem = context.InitializationData.FileSystem;

        string tmdbPath = fileSystem.Path.Combine(context.BasePath, "tmdb.json");
        if (!await fileSystem.File.Exists(tmdbPath) && context.ImportItem.GetTmdbItemToSerialize() != null)
        {
            await fileSystem.File.WriteAllText(tmdbPath, JsonSerializer.Serialize(context.ImportItem.GetTmdbItemToSerialize(), OGIb.JsonHelper.JsonOptions));
        }
    }
}
