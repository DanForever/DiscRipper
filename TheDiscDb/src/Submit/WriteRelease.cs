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

        if (string.IsNullOrWhiteSpace(context.Release?.ReleaseSlug))
        {
            throw new ArgumentException("Submission must have ReleaseSlug set before writing release data.");
        }

        var fileSystem = context.InitializationData.FileSystem;
        context.ReleaseFolder = fileSystem.Path.Combine(context.BasePath, context.Release.ReleaseSlug);

        await fileSystem.Directory.CreateDirectory(context.ReleaseFolder);

        await Download(context, context.Release.FrontCoverUrl, "front.jpg");
        await Download(context, context.Release.BackCoverUrl, "back.jpg");


        string releaseFile = fileSystem.Path.Combine(context.ReleaseFolder, OGTddb.ImportModels.ReleaseFile.Filename);
        if (!await fileSystem.Directory.Exists(releaseFile))
        {
            int year = context.Release.PublicationDate?.Year ?? context.Year;

            var release = new OGTddb.ImportModels.ReleaseFile
            {
                Title = context.Release.EditionName?.Trim(),
                SortTitle = $"{year} {GetSortTitle(context.Release.EditionName)}",
                Slug = context.Release.ReleaseSlug.Trim(),
                Upc = context.Release.UPC?.Trim(),
                Locale = context.Release.Locale,
                Year = year,
                RegionCode = $"{(int)context.Release.RegionCode}",
                Asin = context.Release.ASIN?.Trim(),
                ReleaseDate = context.Release.PublicationDate ?? default,
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
