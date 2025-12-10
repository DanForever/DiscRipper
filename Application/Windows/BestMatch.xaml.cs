using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DiscRipper.Windows;

/// <summary>
/// This window presents the user with what it thinks is the correct match from TheDiscDb based on the information retrieved from MakeMkv
/// The user can then choose to rip the disc using that match, or switch to a different match (if there are any)
/// </summary>
internal partial class BestMatch : Window
{
	#region Private properties

	private List<Mapped.Disc> MappedDiscs { get; init; }
	private MakeMkv.Drive Drive { get; set; }
	private MappedTitleViewModelList? MappedTitleViewModelList { get; set; }
	private int ActiveDiscIndex { get; set; } = 0;

	#endregion Private properties

	public required SubmitRelease SubmitNewDisc { get; init; }

	#region C-Tor

	internal BestMatch(List<Mapped.Disc> mappedDiscs, MakeMkv.Drive drive)
	{
		InitializeComponent();

		MappedDiscs = mappedDiscs;
		Drive = drive;

		if (MappedDiscs.Count == 0)
			return;

		SetDisc(0);
	}

	#endregion C-Tor

	#region Private methods

	private void SetDisc(int discIndex)
	{
		ActiveDiscIndex = discIndex;

		// Enable or disable the prev/next buttons if their associated indices line up with another match
		int prevIndex = discIndex - 1;
		int nextIndex = discIndex + 1;

		if (prevIndex < 0)
			Previous.IsEnabled = false;
		else
			Previous.IsEnabled = true;

		if (nextIndex >= MappedDiscs.Count)
			Next.IsEnabled = false;
		else
			Next.IsEnabled = true;

		if (ActiveDiscIndex >= MappedDiscs.Count || ActiveDiscIndex < 0)
			return;

		Mapped.Disc disc = MappedDiscs[ActiveDiscIndex];

		DataContext = disc;

		// Grab the cover art for the specified disc and convert it into a format our image control can display
		string url = $"https://thediscdb.com/images/{disc.tddbRelease.ImageUrl}";

		var bitmap = new BitmapImage();
		bitmap.BeginInit();
		bitmap.UriSource = new Uri(url, UriKind.Absolute);
		bitmap.CacheOption = BitmapCacheOption.OnLoad;
		bitmap.EndInit();

		CoverImage.Source = bitmap;

		// Display the matched disc titles to the user so that they may choose to only rip some of them
		MappedTitleViewModelList = new MappedTitleViewModelList(disc.MatchedTitles);
		FoundTitles.ItemsSource = MappedTitleViewModelList;
	}

	#endregion Private methods

	#region Event handlers

	private void Cancel_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private async void Rip_Click(object sender, RoutedEventArgs e)
	{
		// If no path has been specified by the user, gracefully stop processing
		//todo: present the user with some sort of error message
		if (string.IsNullOrWhiteSpace(OutputPath.Text))
			return;

		if (MappedTitleViewModelList == null)
			return;

		Mapped.Disc disc = MappedDiscs[ActiveDiscIndex];

		// Append a properly named folder for the disc we're ripping to the path specified by the user
		string destinationFolder = Path.Join(OutputPath.Text, disc.tddbNode.DirectoryName);

		MakeMkv.Runner runner = new()
		{
			MakeMkvDir = Settings.Default.MakeMkvInstallFolder,
			SynchronizationContext = SynchronizationContext.Current!,
		};

		MakeMkvFeedback feedback = new() { Title = "Ripping disc", Owner = this };
		feedback += runner.Log;
		feedback.Show();

		// Disable this window so that the user can't interact with it
		// once the ripping process has begun
		IsEnabled = false;

		foreach (var titleViewModel in MappedTitleViewModelList)
		{
			if (titleViewModel.Rip)
			{
				var title = titleViewModel.MappedTitle;
				string titleDestinationFolder;

				// Extras want to go into dedicated sub-folders, so if the title is an extra, figure out the path for it here
				switch (title.tddbTitle.ItemType)
				{
				case "DeletedScene":
					titleDestinationFolder = Path.Join(destinationFolder, "Deleted Scenes");
					break;

				case "Trailer":
					titleDestinationFolder = Path.Join(destinationFolder, "Trailers");
					break;

				// "TheDiscDB" has a generic "extra" category, so we can't differentiate BTS/Featurettes/Interviews
				case "Extra":
					titleDestinationFolder = Path.Join(destinationFolder, "Behind The Scenes");
					break;

				default:
					titleDestinationFolder = destinationFolder;
					break;
				}

				// Ensure the directory exists (MakeMkv won't do it for us)
				Directory.CreateDirectory(titleDestinationFolder);

				// Run MakeMkv
				Task runnerTask = runner.Mkv(Drive.Index, title.mmkvTitle.Index, titleDestinationFolder);

				// Wait for MakeMkv to finish
				await runnerTask;

				// The MakeMkv command line doesn't let us specify what to call the
				// title being ripped, so we need to rename it after the fact
				string sourcePath = Path.Join(titleDestinationFolder, title.mmkvTitle.Filename);
				string destPath = Path.Join(titleDestinationFolder, title.tddbTitle.Item.Filename);
				File.Move(sourcePath, destPath);
			}
		}

		// All finished, the window can now be re-enabled
		// todo: automatically close the window, present some sort of "finished!" prompt?
		IsEnabled = true;
	}

	/// <summary>
	/// Displays a nice system-level folder picker to the user
	/// </summary>
	private void PickFolder_Click(object sender, RoutedEventArgs e)
	{
		Microsoft.Win32.OpenFolderDialog dialog = new()
		{
			Multiselect = false,
			Title = "Select a folder"
		};

		if (dialog.ShowDialog() == true)
		{
			string folderPath = dialog.FolderName;
			OutputPath.Text = folderPath;
		}
	}

	#endregion Private methods

	private void Previous_Click(object sender, RoutedEventArgs e)
	{
		SetDisc(ActiveDiscIndex - 1);
	}

	private void Next_Click(object sender, RoutedEventArgs e)
	{
		SetDisc(ActiveDiscIndex + 1);
	}

	private void AddNewRelease_Click(object sender, RoutedEventArgs e)
	{
		SubmitNewDisc.Show();
	}

	private void AddDiscToRelease_Click(object sender, RoutedEventArgs e)
	{

	}
}
