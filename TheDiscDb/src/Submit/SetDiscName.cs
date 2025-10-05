using System.Diagnostics;

namespace DiscRipper.TheDiscDb.Submit;

public class SetDiscName : IStep
{
    public async Task Run(SubmissionContext context)
    {
        if(context.ReleaseFolder == null)
        {
            throw new ArgumentException("ReleaseFolder must be set in context before setting disc name.");
        }

        await DoSetDiscName(context);
    }

    private static async Task DoSetDiscName(SubmissionContext context)
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
