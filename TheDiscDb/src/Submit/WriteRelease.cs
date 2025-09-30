using System.Text.Json;

using Fantastic.FileSystem;

using global::ImportBuddy;

using OGTddb = global::TheDiscDb;

namespace DiscRipper.TheDiscDb.Submit;

public class WriteRelease : IStep
{
    public async Task Run(SubmissionContext context)
    {
        if (string.IsNullOrWhiteSpace(context.BasePath))
        {
            throw new ArgumentException("BasePath must be set in context before writing release data.");
        }

        if (string.IsNullOrWhiteSpace(context.Submission.ReleaseSlug))
        {
            throw new ArgumentException("Submission must have ReleaseSlug set before writing release data.");
        }

        var fileSystem = context.InitializationData.FileSystem;
        context.ReleaseFolder = fileSystem.Path.Combine(context.BasePath, context.Submission.ReleaseSlug);

        await fileSystem.Directory.CreateDirectory(context.ReleaseFolder);

        await Download(context, context.Submission.FrontCoverUrl, "front.jpg");
        await Download(context, context.Submission.BackCoverUrl, "back.jpg");


        string releaseFile = fileSystem.Path.Combine(context.ReleaseFolder, OGTddb.ImportModels.ReleaseFile.Filename);
        if (!await fileSystem.Directory.Exists(releaseFile))
        {
            int year = context.Submission.PublicationDate?.Year ?? context.Year;

            var release = new OGTddb.ImportModels.ReleaseFile
            {
                Title = context.Submission.EditionName,
                SortTitle = $"{year} {GetSortTitle(context.Submission.EditionName)}",
                Slug = context.Submission.ReleaseSlug,
                Upc = context.Submission.UPC,
                Locale = context.Submission.Locale,
                Year = year,
                RegionCode = $"{(int)context.Submission.RegionCode}",
                Asin = context.Submission.ASIN,
                ReleaseDate = context.Submission.PublicationDate ?? default,
                DateAdded = DateTimeOffset.UtcNow.Date
            };

            string json = JsonSerializer.Serialize(release, JsonHelper.JsonOptions);
            await fileSystem.File.WriteAllText(releaseFile, json);
        }
    }

    private async Task Download(SubmissionContext context, string? url, string filename)
    {
        if (string.IsNullOrEmpty(url))
            return;

        System.Diagnostics.Debug.Assert(context.ReleaseFolder != null);

        var fileSystem = context.InitializationData.FileSystem;
        string destinationPath = fileSystem.Path.Combine(context.ReleaseFolder, filename);
        await context.InitializationData.HttpClient.Download(fileSystem, url, destinationPath);
    }

    private static string? GetSortTitle(string? title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return title;
        }

        if (title.StartsWith("the", StringComparison.OrdinalIgnoreCase))
        {
            return title["the".Length..].Trim() + ", The";
        }
        else
        {
            return title;
        }
    }
}
