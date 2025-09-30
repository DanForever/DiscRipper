using global::ImportBuddy;

namespace DiscRipper.TheDiscDb.Submit;

public class CreateDirectory : IStep
{
    public async Task Run(SubmissionContext context)
    {
        if (string.IsNullOrWhiteSpace(context.InitializationData.WrappedOptions.Value.DataRepositoryPath))
        {
            throw new ArgumentException("Repository directory path is not set");
        }

        if (context.Submission.MediaType == null)
        {
            throw new ArgumentException("Submission MediaType must be set in context before building metadata.");
        }

        string folderName = $"{context.InitializationData.FileSystem.CleanPath(context.Metadata!.Title!)} ({context.Year})";
        string subFolderName = context.Submission.MediaType.ToLower();

        context.BasePath = context.InitializationData.FileSystem.Path.Combine(context.InitializationData.WrappedOptions.Value.DataRepositoryPath, subFolderName, folderName);

        var directory = context.InitializationData.FileSystem.Directory;

        if (!(await directory.Exists(context.BasePath)))
        {
            await directory.CreateDirectory(context.BasePath);
        }
    }
}
