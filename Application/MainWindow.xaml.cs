using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using DiscRipper.Windows;

namespace DiscRipper
{
	public partial class MainWindow : Window
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
			await ScanDrives();
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
	}
}
