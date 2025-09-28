using System.Text;

using Fantastic.FileSystem;

namespace DiscRipper.TheDiscDb;

public class ReimplementedImportBuddyFunctionality
{
    public static async Task CreateDiscSummaryFile(CopiedImportBuddyFunctions copiedImportBuddyFunctions, string releaseFolder, Submission submission)
    {
        var discName = await copiedImportBuddyFunctions.GetDiscName(releaseFolder);

        string makeMkvLogPath = copiedImportBuddyFunctions.FileSystem.Path.Combine(releaseFolder, $"{discName.Name}.txt");
        if (!await copiedImportBuddyFunctions.FileSystem.File.Exists(makeMkvLogPath))
        {
            var summaryContents = new StringBuilder();

            foreach(var title in submission.Titles)
            {
                summaryContents.AppendLine(title.Format);
                summaryContents.AppendLine();
            }

            string summaryPath = copiedImportBuddyFunctions.FileSystem.Path.Combine(releaseFolder, $"{discName.Name}-summary.txt");
            if (!await copiedImportBuddyFunctions.FileSystem.File.Exists(summaryPath))
            {
                await copiedImportBuddyFunctions.FileSystem.File.WriteAllText(summaryPath, summaryContents.ToString());
            }
        }
}
}