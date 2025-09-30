using System.Text.Json;

using Fantastic.FileSystem;

using OGIb = global::ImportBuddy;
using OGTddb = global::TheDiscDb;

namespace DiscRipper.TheDiscDb.Submit;

public class WriteMetadata : IStep
{
    public async Task Run(SubmissionContext context)
    {
        if (context.ImportItem == null)
        {
            throw new ArgumentException("ImportItem must be set in context before writing TMDB data.");
        }

        if (context.BasePath == null)
        {
            throw new ArgumentException("BasePath must be set in context before writing TMDB data.");
        }

        var fileSystem = context.InitializationData.FileSystem;

        string metadataPath = fileSystem.Path.Combine(context.BasePath, OGTddb.ImportModels.MetadataFile.Filename);
        if (!await fileSystem.File.Exists(metadataPath))
        {
            await fileSystem.File.WriteAllText(metadataPath, JsonSerializer.Serialize(context.Metadata, OGIb.JsonHelper.JsonOptions));
        }
    }
}
