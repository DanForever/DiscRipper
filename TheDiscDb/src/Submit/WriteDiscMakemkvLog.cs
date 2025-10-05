using Fantastic.FileSystem;

namespace DiscRipper.TheDiscDb.Submit;

public class WriteDiscMakemkvLog : IStep
{
    public async Task Run(SubmissionContext context)
    {
        if(context.ReleaseFolder == null)
        {
            throw new ArgumentException("ReleaseFolder must be set in context before writing makemkv log.");
        }

        if (context.DiscName == null)
        {
            throw new ArgumentException("DiscName must be set in context before writing makemkv log.");
        }

        string logFilePath = context.InitializationData.FileSystem.Path.Combine(context.ReleaseFolder, $"{context.DiscName}.txt");
        await context.InitializationData.FileSystem.File.WriteAllText(logFilePath, context.Log);
    }
}
