using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DiscRipper.Windows;

internal partial class MainWindow
{
	#region Private fields

	private readonly ViewModel.MainWindowViewModel _viewModel = new();
	private readonly TheDiscDb2.Querier _querier = new();
	private readonly TitleMapper _titleMapper = new();

	#endregion Private fields

	#region C-Tor

	public MainWindow()
	{
		InitializeComponent();

		DataContext = _viewModel;

#if DEBUG
		DebugMenu.Visibility = Visibility.Visible;
#else
		DebugMenu.Visibility = Visibility.Collapsed;
#endif

		
		Loaded += MainWindow_Loaded;
	}

	#endregion C-Tor

	#region Event handlers

	private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
	{
		//await ScanDiscDummy();

		// On startup we want to automatically populate the list of drives,
		// but we can't do it in the constructor because the gui hasn't yet
		// been initialized, and it can't be made async, so we do it here instead
		//await ScanDrives();
	}

	private async void Drives_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;
		if (row != null && row.Item is MakeMkv.Drive drive)
		{
			// See what's on the disc in the selected drive
			await ScanDisc(drive);
		}
	}

	private void Settings_Click(object sender, RoutedEventArgs e)
	{
		EditSettings settings = new() { Owner = this };
		settings.Show();
	}

	private async void RescanDrives_Click(object sender, RoutedEventArgs e)
	{
		await ScanDrives();
	}

	private void Exit_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void LoadPreviousSession_Click(object sender, RoutedEventArgs e)
	{
		SessionPicker picker = new() { Owner = this };
		picker.Show();
	}

	private async void Rip_Click(object sender, RoutedEventArgs e)
	{
		if (Drives.SelectedItem is MakeMkv.Drive drive)
		{
			// See what's on the disc in the selected drive
			await ScanDisc(drive);
		}
	}

	private async void SubmitNewRelease_Click(object sender, RoutedEventArgs e)
	{
		if (Drives.SelectedItem is MakeMkv.Drive drive)
		{
			MakeMkv.Runner runner = new()
			{
				MakeMkvDir = Settings.Default.MakeMkvInstallFolder,
				SynchronizationContext = SynchronizationContext.Current!,
			};

			Feedback += runner.Log;
			await runner.Info(drive.Index, Settings.Default.MakeMkvMinimumTitleLength);

			MakeMkv.TitleEngine titleEngine = new();
			await titleEngine.Read(runner.Log);

			SubmitRelease submitNewDisc = new(runner.Log, titleEngine.Titles.ToList(), drive) { Owner = this };
			submitNewDisc.Show();
		}
	}

	private async void AddNewDisc_Click(object sender, RoutedEventArgs e)
	{
		if (Drives.SelectedItem is MakeMkv.Drive drive)
		{
			MakeMkv.Runner runner = new()
			{
				MakeMkvDir = Settings.Default.MakeMkvInstallFolder,
				SynchronizationContext = SynchronizationContext.Current!,
			};

			Feedback += runner.Log;
			await runner.Info(drive.Index, Settings.Default.MakeMkvMinimumTitleLength);

			MakeMkv.TitleEngine titleEngine = new();
			await titleEngine.Read(runner.Log);

			ReleasePicker picker = new() { Owner = this };
			picker.ShowDialog();

			if (picker.Release is not null)
			{
				SubmitAdditionalDisc submitSecondDisc = new(runner.Log, titleEngine.Titles.ToList(), picker.Release, drive) { Owner = this };
				submitSecondDisc.Show();
			}
		}
	}

	#endregion Event handlers

	#region Private methods

	private async Task ScanDrives()
	{
		MakeMkv.Runner runner = new()
		{
			MakeMkvDir = Settings.Default.MakeMkvInstallFolder,
			SynchronizationContext = SynchronizationContext.Current!,
		};

		Feedback += runner.Log;
		await runner.Drives();

		foreach (var item in runner.Log.Drives)
			_viewModel.Drives.Add(item);
	}

	private async Task ScanDisc(MakeMkv.Drive drive)
	{
		MakeMkv.Runner runner = new()
		{
			MakeMkvDir = Settings.Default.MakeMkvInstallFolder,
			SynchronizationContext = SynchronizationContext.Current!,
		};

		Feedback += runner.Log;
		await runner.Info(drive.Index, Settings.Default.MakeMkvMinimumTitleLength);

		MakeMkv.TitleEngine titleEngine = new();
		await titleEngine.Read(runner.Log);

		await _querier.Query(titleEngine.LongestTitle?.Duration);

		List<Mapped.Disc> mappedDiscs = TitleMapper.Map(titleEngine.Titles.ToList(), _querier.Nodes);

		SubmitRelease submitNewDisc = new(runner.Log, titleEngine.Titles.ToList(), drive) { Owner = this };

		if (mappedDiscs.Count > 0)
		{
			BestMatch bestMatch = new(mappedDiscs, drive) { Owner = this, SubmitNewDisc = submitNewDisc };
			bestMatch.Show();
		}
		else
		{
			submitNewDisc.Show();
		}
	}

	private async Task ScanDiscDummy()
	{
		MakeMkv.Runner runner = new()
		{
			MakeMkvDir = Settings.Default.MakeMkvInstallFolder,
			SynchronizationContext = SynchronizationContext.Current!,
		};

		Feedback += runner.Log;
		await runner.RunPreloaded(DummyData.TellNoOne);

		MakeMkv.TitleEngine titleEngine = new();
		await titleEngine.Read(runner.Log);

		List<Mapped.Disc> mappedDiscs = TitleMapper.Map(titleEngine.Titles.ToList(), _querier.Nodes);
		SubmitRelease submitNewDisc = new(runner.Log, titleEngine.Titles.ToList(), null) { Owner = this };

		if (mappedDiscs.Count > 0)
		{
			BestMatch bestMatch = new(mappedDiscs, null!) { Owner = this, SubmitNewDisc = submitNewDisc };
			bestMatch.Show();
		}
		else
		{
			submitNewDisc.Show();
		}
	}

	#endregion Private methods

	private async void TestDisc_Click(object sender, RoutedEventArgs e)
	{
		MakeMkv.Runner runner = new()
		{
			MakeMkvDir = Settings.Default.MakeMkvInstallFolder,
			SynchronizationContext = SynchronizationContext.Current!,
		};

		Feedback += runner.Log;
		await runner.RunPreloaded(DummyData.TellNoOne);

		MakeMkv.TitleEngine titleEngine = new();
		await titleEngine.Read(runner.Log);

		List<Mapped.Disc> mappedDiscs = TitleMapper.Map(titleEngine.Titles.ToList(), _querier.Nodes);

		ReleasePicker picker = new() { Owner = this };
		picker.ShowDialog();

		if (picker.Release is not null)
		{
			SubmitAdditionalDisc submitSecondDisc = new(runner.Log, titleEngine.Titles.ToList(), picker.Release) { Owner = this };
			submitSecondDisc.Show();
		}
	}

	private async void TestRip_Click(object sender, RoutedEventArgs e)
	{
		MakeMkv.Runner runner = new()
		{
			MakeMkvDir = Settings.Default.MakeMkvInstallFolder,
			SynchronizationContext = SynchronizationContext.Current!,
		};

		Feedback += runner.Log;
		await runner.RunPreloaded(DummyData.TellNoOne);

		MakeMkv.TitleEngine titleEngine = new();
		await titleEngine.Read(runner.Log);

		await _querier.Query(titleEngine.LongestTitle?.Duration);

		List<Mapped.Disc> mappedDiscs = TitleMapper.Map(titleEngine.Titles.ToList(), _querier.Nodes);

		SubmitRelease submitNewDisc = new(runner.Log, titleEngine.Titles.ToList()) { Owner = this };

		if (mappedDiscs.Count > 0)
		{
			BestMatch bestMatch = new(mappedDiscs) { Owner = this, SubmitNewDisc = submitNewDisc };
			bestMatch.Show();
		}
		else
		{
			submitNewDisc.Show();
		}
	}

	private async void TestRelease_Click(object sender, RoutedEventArgs e)
	{
		MakeMkv.Runner runner = new()
		{
			MakeMkvDir = Settings.Default.MakeMkvInstallFolder,
			SynchronizationContext = SynchronizationContext.Current!,
		};

		Feedback += runner.Log;
		await runner.RunPreloaded(DummyData.TellNoOne);

		MakeMkv.TitleEngine titleEngine = new();
		await titleEngine.Read(runner.Log);

		List<Mapped.Disc> mappedDiscs = TitleMapper.Map(titleEngine.Titles.ToList(), _querier.Nodes);

		SubmitRelease submitNewDisc = new(runner.Log, titleEngine.Titles.ToList(), null) { Owner = this };
		submitNewDisc.Show();
	}

	private async void TestGuidedRelease_Click(object sender, RoutedEventArgs e)
	{
		MakeMkv.Runner runner = new()
		{
			MakeMkvDir = Settings.Default.MakeMkvInstallFolder,
			SynchronizationContext = SynchronizationContext.Current!,
		};

		Feedback += runner.Log;
		await runner.RunPreloaded(DummyData.TellNoOne);

		MakeMkv.TitleEngine titleEngine = new();
		await titleEngine.Read(runner.Log);

		List<Mapped.Disc> mappedDiscs = TitleMapper.Map(titleEngine.Titles.ToList(), _querier.Nodes);

		//Guided.Tmdb window = new() { Owner = this };
		//window.ShowDialog();

		//Guided.MediaType window = new() { Owner = this };
		//window.ShowDialog();

		Sessions.Session session = Sessions.SessionManager.Instance.Value.CreateSession(titleEngine.Titles.ToList());
		IList<ViewModel.SubmissionTitle> submissionTitles = session.Disc.Titles.Select(t => new ViewModel.SubmissionTitle { Model = t }).ToList();

		ViewModel.Submission submission = new()
		{
			DiscModel = session.Disc,
			Model = session.Release,
			Titles = submissionTitles,
		};

		Base.GuidedStep tmdb = new()
		{
			Owner = this,
			Title = "Step One - TMDB ID",
			Description = "The first bit of information we require is the ID for the show or movie you want to submit to TheDiscDb. This simply involves going to the website (which you can do by clicking the button below), and then navigating to the page for the movie or show in question. After that, you can copy and paste the URL into the text field below.",
			InnerContent = new Controls.Guided.Tmdb(),

			Submission = submission,
			Session = session,
			Log = runner.Log,
		};

		Base.GuidedStep mediaType = new()
		{
			Owner = this,
			Title = "Step two - Select the type of media on the disc",
			Description = """
				Does this disc contain a movie (or movie extras), or is it part of a TV series/show?
				If, in the previous step, you copied and pasted the URL of the TMDB page, then we've probably automatically filled this in already, in that case, just verify that we got it right and move on to the next step.
				""",
			InnerContent = new Controls.Guided.MediaType(),

			Submission = submission,
			Session = session,
			Log = runner.Log,
		};

		Base.GuidedStep upc = new()
		{
			Owner = this,
			Title = "Step three - UPC",
			Description = """
				Here we want the Universal product code, or "UPC". This should be the number underneath the barcode on the back of the box. It should also be possible to find this on the amazon product page under "Manufacturer reference" in the "Product details" section.
				""",
			InnerContent = new Controls.Guided.Upc(),

			Submission = submission,
			Session = session,
			Log = runner.Log,
		};

		Base.GuidedStep asin = new()
		{
			Owner = this,
			Title = "Step four - ASIN",
			Description = """
				Looking for another form of ID, but this time it's unique to amazon - "Amazon Standard Identification Number".
				Like the UPC, this one can be also found on the amazon product page in the "Product details" section, simply labelled "ASIN".
				However, again you can also simply copy and paste the url to the amazon product page directly into the textbox below and we'll extract it from that automatically.
				""",
			InnerContent = new Controls.Guided.Asin(),

			Submission = submission,
			Session = session,
			Log = runner.Log,
		};

		Base.GuidedStep releaseDate = new()
		{
			Owner = this,
			Title = "Step five - Release publication date",
			Description = """
				The date that this particular disc edition was released (so not the date for when the movie first entered cinemas).
				Just like previously, it should be possible to find this information on the amazon product page under "Release date" in the "Product details" section.
				""",
			InnerContent = new Controls.Guided.PublicationDate(),

			Submission = submission,
			Session = session,
			Log = runner.Log,
		};

		Base.GuidedStep coverArt = new()
		{
			Owner = this,
			Title = "Step six - Cover art",
			Description = """
				Here we would like you to specify urls to where we can download images for the front and back cover of the box for this release.
				Search for the cover art on the internet, and when you find it, right click -> "copy image link", and then paste that in the appropriate field below.
				Usually, the artwork should be available on the amazon product site, but if it's not there, you may also have luck at blu-ray.com.
				""",
			InnerContent = new Controls.Guided.CoverArt(),

			Submission = submission,
			Session = session,
			Log = runner.Log,
		};

		Base.GuidedStep regionCode = new()
		{
			Owner = this,
			Title = "Step six - Region locking code",
			Description = """
				Is this disc locked to a specific region? Usually you can find the region code on the back of the box, it will be a letter (A, B or C) inside a globe icon. If there is no such icon, then the disc is most likely region free.
				Sites like blu-ray.com usually also have the region code listed on the product page, so if you have trouble finding it on the box, you can also try looking there.

				You can find out more about the different region codes and what they mean at Wikipedia.
				""",
			InnerContent = new Controls.Guided.RegionCode(),

			Submission = submission,
			Session = session,
			Log = runner.Log,
		};


		//Base.GuidedStep coverArt = new()
		//{
		//	Owner = this,
		//	Title = "Step six - Cover art",
		//	Description = """
		//		Here we would like you to specify urls to where we can download images for the front and back cover of the box for this release.
		//		Search for the cover art on the internet, and when you find it, right click -> "copy image link", and then paste that in the appropriate field below.
		//		Usually, the artwork should be available on the amazon product site, but if it's not there, you may also have luck at blu-ray.com.
		//		""",
		//	InnerContent = new Controls.Guided.CoverArt(),

		//	Submission = submission,
		//	Session = session,
		//	Log = runner.Log,
		//};

		regionCode.DataContext = submission;
		regionCode.ShowDialog();
	}

	private void SwitchTheme_Click(object sender, RoutedEventArgs e)
	{
		Wpf.Ui.Appearance.ApplicationTheme currentAppTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme();

		switch (currentAppTheme)
		{
		case Wpf.Ui.Appearance.ApplicationTheme.Light:
			Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark, Wpf.Ui.Controls.WindowBackdropType.None);
			break;

		case Wpf.Ui.Appearance.ApplicationTheme.Dark:
			Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Light, Wpf.Ui.Controls.WindowBackdropType.None);
			break;
		}
	}

	private void GotoGithub_Click(object sender, RoutedEventArgs e)
	{
		const string url = "https://github.com/DanForever/DiscRipper";

		try
		{
			var psi = new System.Diagnostics.ProcessStartInfo
			{
				FileName = url,
				UseShellExecute = true
			};
			System.Diagnostics.Process.Start(psi);
		}
		catch (System.Exception ex)
		{
			MessageBox.Show(this, $"Unable to open browser: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
