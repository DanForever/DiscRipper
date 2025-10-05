using System.Text.RegularExpressions;

using global::ImportBuddy;

namespace DiscRipper.TheDiscDb.Submit;

public partial class GenerateHashData : IStep
{
    [GeneratedRegex(@"(\w):\\?")]
    private static partial Regex PathRegex();

    public async Task Run(SubmissionContext context)
    {
        if(context.DrivePath == null)
        {
            return;
        }

        char driveLetter = PathRegex().Match(context.DrivePath).Value[0];

        context.DiscHashInfo = await context.InitializationData.FileSystem.HashMediaDisc(driveLetter);
    }
}
