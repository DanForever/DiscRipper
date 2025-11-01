using System.Text.RegularExpressions;

using DiscRipper.TheDiscDb.Types;

using global::ImportBuddy;

namespace DiscRipper.TheDiscDb.Submit;

public partial class GenerateHashData : IStep
{
	[GeneratedRegex(@"(\w):\\?")]
	private static partial Regex PathRegex();

	public async Task Run(SubmissionContext context)
	{
		if (context.DrivePath == null)
		{
			return;
		}

		// Figure out which drive we're meant to read from
		char driveLetter = PathRegex().Match(context.DrivePath).Value[0];

		// Generate the hash data
		var hashData = await context.InitializationData.FileSystem.HashMediaDisc(driveLetter);

		// Convert it to something the main project can read and serialize
		context.DiscHash = hashData?.ToDiscRipperType();
	}
}
