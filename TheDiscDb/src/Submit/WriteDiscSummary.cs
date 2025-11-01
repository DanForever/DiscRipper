using System.Text;

using Fantastic.FileSystem;

namespace DiscRipper.TheDiscDb.Submit;

public class WriteDiscSummary : IStep
{
    public async Task Run(SubmissionContext context)
    {
        StringBuilder contents = new();

        foreach(var title in context.Submission.Titles)
        {
            if (title.Type == DiscRipper.Types.TitleType.Ignore)
                continue;

            contents.Append(title.Format);
            contents.AppendLine(string.Empty);
            contents.AppendLine(string.Empty);
        }

        if(context.ReleaseFolder is null)
            throw new ArgumentException("Release folder is not set.");

        string summaryPath = context.InitializationData.FileSystem.Path.Combine(context.ReleaseFolder, $"{context.DiscName}-summary.txt");
        await context.InitializationData.FileSystem.File.WriteAllText(summaryPath, contents.ToString());
    }
}
