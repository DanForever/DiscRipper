using System.Diagnostics;

using DiscRipper.TheDiscDb.Types;

using Fantastic.FileSystem;

using OGTddb = global::TheDiscDb;

namespace DiscRipper.TheDiscDb.Submit;

public class AppendHashesToMakemkvLog : IStep
{
    private readonly List<string> _logLines = [];

    public async Task Run(SubmissionContext context)
    {
        if(context.ReleaseFolder == null)
        {
            throw new ArgumentException("ReleaseFolder must be set in context before appending to makemkv log.");
        }

        if (context.DiscHash == null)
        {
            throw new ArgumentException("DiscHashInfo must be set in context before appending to makemkv log.");
        }

        AddBaseLogLines(context);
        AddHashLines(context.DiscHash.ToImportBuddyType());
        await WriteLog(context);
    }

    private void AddBaseLogLines(SubmissionContext context)
    {
        _logLines.Add(context.Log);
    }

    private void AddHashLines(OGTddb.Core.DiscHash.DiscHashInfo hashInfo)
    {
        foreach (var info in hashInfo.Files)
        {
            string line = $"HSH:{info.Index},{info.Name},{info.CreationTime},{info.Size}";

            _logLines.Add(line);
        }
    }

    private async Task WriteLog(SubmissionContext context)
    {
        Debug.Assert(context.ReleaseFolder != null);

        string logFilePath = context.InitializationData.FileSystem.Path.Combine(context.ReleaseFolder, $"{context.DiscName}.txt");
        await context.InitializationData.FileSystem.File.WriteAllLines(logFilePath, _logLines);
    }
}
