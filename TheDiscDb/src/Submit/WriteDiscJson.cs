using System.Text.Json;

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
            var discJsonFile = new OGTddb.ImportModels.DiscFile
            {
                Index = context.DiscIndex,
                Slug = context.Submission.DiscSlug,
                Name = context.Submission.DiscTitle,
                Format = context.Submission.DiscFormat,
                ContentHash = context.DiscHashInfo?.Hash
            };

            await fileSystem.File.WriteAllText(discJsonFilePath, JsonSerializer.Serialize(discJsonFile, OGIb.JsonHelper.JsonOptions));
        }
    }
}
