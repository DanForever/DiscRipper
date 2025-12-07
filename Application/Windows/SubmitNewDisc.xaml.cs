using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using DiscRipper.Sessions;
using DiscRipper.TheDiscDb.Types;
using DiscRipper.ViewModel;

using Microsoft.Extensions.Options;

namespace DiscRipper
{
	internal partial class SubmitNewDisc : Window
	{
		public Session Session { get; init; }
		public ViewModel.Submission Submission { get; init; }
		public MakeMkv.Log Log { get; init; }
		public MakeMkv.Drive? Drive { get; init; }

		public static string[] MediaTypes { get; } = ["Movie", "Series"];
		public static string[] DiscFormats { get; } = ["Blu-Ray", "UHD", "DVD"];
		public static IEnumerable<CultureInfo> AllRegions { get; private set; }

		static SubmitNewDisc()
		{
			AllRegions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).OrderBy(c => c.DisplayName);
		}

		public SubmitNewDisc(Session session, MakeMkv.Log log)
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

		public SubmitNewDisc(MakeMkv.Log log, List<Types.Title> titles, MakeMkv.Drive? drive = null) : this(SessionManager.Instance.Value.CreateSession(titles), log)
		{
			Session.Log = Log.ExportRawLog();

			// sensible defaults
			Submission.Locale = CultureInfo.CurrentCulture.Name;
			Submission.MediaType = MediaTypes[0];

			Drive = drive;
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
				//new TheDiscDb.Submit.GenerateHashData(),
				new TheDiscDb.Submit.TmdbFetch(),
				new TheDiscDb.Submit.BuildMetadata(),
				new TheDiscDb.Submit.CreateDirectory(),
				new TheDiscDb.Submit.SeriesFilenames(),
				new TheDiscDb.Submit.DownloadPoster(),
				new TheDiscDb.Submit.WriteTmdb(),
				new TheDiscDb.Submit.WriteImdb(),
				new TheDiscDb.Submit.WriteMetadata(),
				new TheDiscDb.Submit.WriteRelease(),
				new TheDiscDb.Submit.SetDiscName(),
				//new TheDiscDb.Submit.WriteDiscMakemkvLog(), // Writing the makemkv log is currently done in AppendHashesToMakemkvLog because the "fantastic" filesystem doesn't actually allow for appending to files
				new TheDiscDb.Submit.WriteDiscSummary(),
				new TheDiscDb.Submit.AppendHashesToMakemkvLog(),
				new TheDiscDb.Submit.WriteDiscJson(),
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
}
