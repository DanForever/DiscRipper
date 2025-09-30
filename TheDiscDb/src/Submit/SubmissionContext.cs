using DiscRipper.TheDiscDb.ImportBuddy;

using OGIb = global::ImportBuddy;
using OGTddb = global::TheDiscDb;

namespace DiscRipper.TheDiscDb.Submit;

public class SubmissionContext
{
    public required Submission Submission { get; init; }
    public required InitializationData InitializationData { get; init; }

    public OGIb.ImportItem? ImportItem { get; set; }
    public OGTddb.ImportModels.MetadataFile? Metadata { get; set; }

    public string? BasePath { get; set; }
    public int Year { get; set; }
    public string? ReleaseFolder { get; set; }
}
