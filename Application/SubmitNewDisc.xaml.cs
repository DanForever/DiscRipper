using DiscRipper.Sessions;
using DiscRipper.ViewModel;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

		public SubmitNewDisc(MakeMkv.Log log, List<Types.Title> titles, MakeMkv.Drive? drive = null)
		{
			InitializeComponent();

			IEnumerable<SubmissionTitle> submissionTitles = titles.Select(t => new SubmissionTitle { Model = t });

			Log = log;
			Drive = drive;

			Session = SessionManager.Instance.Value.CreateSession(titles);
			Session.Log = Log.ExportRawLog();

			Submission = new()
			{
				Model = Session.Submission,
				Titles = submissionTitles,

				// sensible defaults
				Locale = "en-US",
				MediaType = MediaTypes[0],
			};

			switch (Drive?.DiscType)
			{
			case MakeMkv.DiscType.DVD:
				Submission.DiscFormat = "DVD";
				break;

			case MakeMkv.DiscType.BD:
			default:
				Submission.DiscFormat = "Blu-Ray";
				break;
			}

			DataContext = Submission;
		}

		public SubmitNewDisc(Session session, MakeMkv.Log log)
		{
			InitializeComponent();

			Log = log;
			Session = session;

			IEnumerable<SubmissionTitle> submissionTitles = Session.Submission.Titles.Select(t => new SubmissionTitle { Model = t });

			Submission = new()
			{
				Model = Session.Submission,
				Titles = submissionTitles,
			};

			DataContext = Submission;
		}
		private async void Submit_Click(object sender, RoutedEventArgs e)
		{
			TheDiscDb.Submit.SubmissionContext context = new()
			{
				Submission = Session.Submission,
				InitializationData = new(Settings.Default.RepositoryFolder),
				Log = Log.ExportRawLog(true),
				DriveIndex = Drive?.Index,
				DrivePath = Drive?.DrivePath,
			};

			TheDiscDb.Submit.IStep[] steps =
			[
				new TheDiscDb.Submit.GenerateHashData(),
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
				return;
			}

			Debug.WriteLine("Done");
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{

		}

		private void Title_Selected(object sender, RoutedEventArgs e)
		{
			var grid = (DataGrid)sender;
			if (grid.SelectedItem != null)
			{
				SelectedTitleDetails.DataContext = grid.SelectedItem;
			}
		}

		private void MediaType_Changed(object sender, SelectionChangedEventArgs e)
		{
			var seasonColumn = TitleDetails.Columns.First(c => c.Header?.ToString() == "Season");
			var episodeColumn = TitleDetails.Columns.First(c => c.Header?.ToString() == "Episode");

			switch (Submission.MediaType)
			{
			case "Series":
				seasonColumn.Visibility = Visibility.Visible;
				episodeColumn.Visibility = Visibility.Visible;
				break;

			case "Movie":
				seasonColumn.Visibility = Visibility.Collapsed;
				episodeColumn.Visibility = Visibility.Collapsed;
				break;
			}
		}
	}
}
