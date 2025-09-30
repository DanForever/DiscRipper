using System.Text.Json;

using Fantastic.FileSystem;

using OGIb = global::ImportBuddy;
namespace DiscRipper.TheDiscDb.Submit;

public class WriteImdb : IStep
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

        string imdbPath = fileSystem.Path.Combine(context.BasePath, "imdb.json");
        if (!await fileSystem.File.Exists(imdbPath) && context.ImportItem.ImdbTitle != null)
        {
            await fileSystem.File.WriteAllText(imdbPath, JsonSerializer.Serialize(context.ImportItem.ImdbTitle, OGIb.JsonHelper.JsonOptions));
        }
    }
}
