namespace DiscRipper.TheDiscDb.Submit;

public interface IStep
{
    Task Run(SubmissionContext context);
}
