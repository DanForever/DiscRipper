using System.Diagnostics;

using Fantastic.FileSystem;

namespace DiscRipper.TheDiscDb.Submit;

public class WriteDiscMakemkvLog : IStep
{
    public async Task Run(SubmissionContext context)
    {
        Debug.Assert(context.ReleaseFolder != null);

        // todo: move to own step class
        await SetDiscName(context);

        string logFilePath = context.InitializationData.FileSystem.Path.Combine(context.ReleaseFolder, $"{context.DiscName}.txt");
        await context.InitializationData.FileSystem.File.WriteAllText(logFilePath, context.Log);
    }

    private static async Task SetDiscName(SubmissionContext context)
    {
        Debug.Assert(context.ReleaseFolder != null);

        var files = await context.InitializationData.FileSystem.Directory.GetFiles(context.ReleaseFolder, "*disc*");

        context.DiscName = "disc01";
        context.DiscIndex = 1;

        for (int i = 1; i < 100; i++)
        {
            context.DiscName = string.Format("disc{0:00}", i);
            context.DiscIndex = i;

            if (files.Any(f => f.Contains(context.DiscName)))
            {
                continue;
            }

            break;
        }
    }
}
