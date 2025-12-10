using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using DiscRipper.Sessions;
using DiscRipper.TheDiscDb.Repo;
using DiscRipper.ViewModel;

namespace DiscRipper.Windows;

internal partial class SubmitAdditionalDisc : Window
{
	public Session Session { get; init; }
	public ViewModel.Submission Submission { get; init; }

	public MakeMkv.Log Log { get; init; }
	public MakeMkv.Drive? Drive { get; init; }

	DiscRipper.Types.Release Release { get; init; }

	public static string[] DiscFormats { get; } = ["Blu-Ray", "UHD", "DVD"];

	public SubmitAdditionalDisc(MakeMkv.Log log, List<Types.Title> titles, DiscRipper.Types.Release release, MakeMkv.Drive? drive = null)
	{
		InitializeComponent();

		IList<SubmissionTitle> submissionTitles = titles.Select(t => new SubmissionTitle { Model = t }).ToList();

		Log = log;
		Drive = drive;
		Release = release;

		Session = SessionManager.Instance.Value.CreateDiscOnlySession(titles);
		Session.Log = Log.ExportRawLog();
		Session.ExistingRelease = release;

		Submission = new()
		{
			DiscModel = Session.Disc,
			Model = Session.Release,
			Titles = submissionTitles,
		};

		DataContext = Submission;

		if (Drive is null)
			return;

		switch (Drive.DiscType)
		{
		case MakeMkv.DiscType.DVD:
			Submission.DiscFormat = "DVD";
			break;

		case MakeMkv.DiscType.BD:
		default:
			Submission.DiscFormat = "Blu-Ray";
			break;
		}

		Loaded += async (_, _) => await GenerateHashData();
	}

	public SubmitAdditionalDisc(Session session, MakeMkv.Log log)
	{
		InitializeComponent();

		Log = log;
		Session = session;

		IList<SubmissionTitle> submissionTitles = Session.Disc.Titles.Select(t => new SubmissionTitle { Model = t }).ToList();

		Submission = new()
		{
			DiscModel = Session.Disc,
			Model = Session.Release,
			Titles = submissionTitles,
		};

		DataContext = Submission;
	}

	private async Task<TheDiscDb.Submit.SubmissionContext> RunSteps(IList<TheDiscDb.Submit.IStep> steps)
	{
		TheDiscDb.Submit.SubmissionContext context = new()
		{
			Disc = Session.Disc,
			Release = Session.Release,
			InitializationData = new(Settings.Default.RepositoryFolder),
			Log = Log.ExportRawLog(true),
			DriveIndex = Drive?.Index,
			DrivePath = Drive?.DrivePath,
			DiscHash = Session.DiscHash,

			ReleaseFolder = Release.Path,
		};

		try
		{
			foreach (var step in steps)
			{
				await step.Run(context);
			}
		}
		catch (ArgumentException ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		return context;
	}

	private async Task GenerateHashData()
	{
		TheDiscDb.Submit.IStep[] steps =
		[
			new TheDiscDb.Submit.GenerateHashData(),
			];

		var context = await RunSteps(steps);
		Session.DiscHash = context.DiscHash;
	}

	private async void Submit_Click(object sender, RoutedEventArgs e)
	{
		TheDiscDb.Submit.IStep[] steps =
		[
			new TheDiscDb.Submit.SetDiscName(),
			new TheDiscDb.Submit.WriteDiscSummary(),
			new TheDiscDb.Submit.AppendHashesToMakemkvLog(),
			new TheDiscDb.Submit.WriteDiscJson()
		];

		await RunSteps(steps);

		Debug.WriteLine("Done");
	}

	private void Cancel_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void MediaType_Changed(object sender, SelectionChangedEventArgs e)
	{
		switch (Submission.MediaType)
		{
		case "Series":
			DiscTitles.SetSeriesPropertiesVisible(true);
			break;

		case "Movie":
			DiscTitles.SetSeriesPropertiesVisible(visible: false);
			break;
		}
	}
}
