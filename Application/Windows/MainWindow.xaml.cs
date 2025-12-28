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

public partial class MainWindow
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

		Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

		DataContext = _viewModel;

#if DEBUG
		DebugMenu.Visibility = Visibility.Visible;
#else
		DebugMenu.Visibility = Visibility.Collapsed;
#endif

		Loaded += MainWindow_Loaded;

	}


	public static void ApplyTheme(FrameworkElement frameworkElement)
	{
		Wpf.Ui.Appearance.ApplicationThemeManager.Apply(frameworkElement);

		void themeChanged(Wpf.Ui.Appearance.ApplicationTheme sender, Color args)
		{
			Wpf.Ui.Appearance.ApplicationThemeManager.Apply(frameworkElement);
			if (frameworkElement is Window window)
			{
				if (window != Wpf.Ui.UiApplication.Current.MainWindow)
				{
					Wpf.Ui.Appearance.WindowBackgroundManager.UpdateBackground(
						window,
						sender,
						Wpf.Ui.Controls.WindowBackdropType.None
					);
				}
			}
		}

		if (frameworkElement.IsLoaded)
		{
			Wpf.Ui.Appearance.ApplicationThemeManager.Changed += themeChanged;
		}

		frameworkElement.Loaded += (s, e) =>
		{
			Wpf.Ui.Appearance.ApplicationThemeManager.Changed += themeChanged;
		};
		frameworkElement.Unloaded += (s, e) =>
		{
			Wpf.Ui.Appearance.ApplicationThemeManager.Changed -= themeChanged;
		};

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

	private void SwitchTheme_Click(object sender, RoutedEventArgs e)
	{
		Wpf.Ui.Appearance.ApplicationTheme currentAppTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme();

		switch (currentAppTheme)
		{
		case Wpf.Ui.Appearance.ApplicationTheme.Light:
			Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark, Wpf.Ui.Controls.WindowBackdropType.None);
			//ThemeService.ApplyThemeMasked(Wpf.Ui.Appearance.ApplicationTheme.Dark);
			break;

		case Wpf.Ui.Appearance.ApplicationTheme.Dark:
			Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Light, Wpf.Ui.Controls.WindowBackdropType.None);
			//ThemeService.ApplyThemeMasked(Wpf.Ui.Appearance.ApplicationTheme.Light);
			break;
		}
	}
}



public static class ThemeService
{
	public static void ApplyTheme2(Wpf.Ui.Appearance.ApplicationTheme theme)
	{
		var old = Application.Current.MainWindow;

		var newWindow = new MainWindow
		{
			Left = -10000, // off-screen
			Top = -10000
		};

		Wpf.Ui.Appearance.ApplicationThemeManager.Apply(theme);

		Application.Current.MainWindow = newWindow;
		newWindow.Show();

		old.Close();

		// move new window to original position
		newWindow.Left = old.Left;
		newWindow.Top = old.Top;
	}

	public static void ApplyTheme(Wpf.Ui.Appearance.ApplicationTheme theme)
	{
		var app = Application.Current;
		if (app?.MainWindow == null)
			return;

		var oldWindow = app.MainWindow;

		// Capture window state
		var state = new WindowStateSnapshot(oldWindow);

		// Apply theme BEFORE creating new window
		Wpf.Ui.Appearance.ApplicationThemeManager.Apply(theme);

		// Create new window
		var newWindow = (Window)System.Activator.CreateInstance(oldWindow.GetType())!;

		newWindow.Opacity = 0;
		newWindow.ContentRendered += (object? sender, System.EventArgs e) => { newWindow.Opacity = 1; };

		// Restore state
		state.Restore(newWindow);

		app.MainWindow = newWindow;
		newWindow.Show();

		oldWindow.Close();
	}

	public static void ApplyThemeMasked(Wpf.Ui.Appearance.ApplicationTheme theme)
	{
		var oldWindow = Application.Current.MainWindow;
		if (oldWindow == null)
			return;

		var snapshot = Capture(oldWindow);
		var mask = new ThemeMaskWindow(snapshot, oldWindow);
		mask.Show();

		Wpf.Ui.Appearance.ApplicationThemeManager.Apply(theme);

		var state = new WindowStateSnapshot(oldWindow);
		var newWindow = (Window)System.Activator.CreateInstance(oldWindow.GetType())!;
		state.Restore(newWindow);

		Application.Current.MainWindow = newWindow;
		newWindow.Show();

		oldWindow.Close();

		// fade out mask
		var fade = new System.Windows.Media.Animation.DoubleAnimation(1, 0, System.TimeSpan.FromMilliseconds(150));

		fade.Completed += (_, _) => mask.Close();
		mask.BeginAnimation(Window.OpacityProperty, fade);
	}

	static BitmapSource Capture(Window window)
	{
		var dpi = VisualTreeHelper.GetDpi(window);

		var rtb = new RenderTargetBitmap(
			(int)(window.ActualWidth * dpi.DpiScaleX),
			(int)(window.ActualHeight * dpi.DpiScaleY),
			dpi.PixelsPerInchX,
			dpi.PixelsPerInchY,
			PixelFormats.Pbgra32);

		rtb.Render(window);
		return rtb;
	}

	private sealed class WindowStateSnapshot
	{
		private readonly WindowState _state;
		private readonly double _top;
		private readonly double _left;
		private readonly double _width;
		private readonly double _height;

		public WindowStateSnapshot(Window window)
		{
			_state = window.WindowState;

			if (_state == WindowState.Normal)
			{
				_top = window.Top;
				_left = window.Left;
				_width = window.Width;
				_height = window.Height;
			}
		}

		public void Restore(Window window)
		{
			window.WindowStartupLocation = WindowStartupLocation.Manual;

			if (_state == WindowState.Normal)
			{
				window.Top = _top;
				window.Left = _left;
				window.Width = _width;
				window.Height = _height;
			}

			window.WindowState = _state;
		}
	}
}
class ThemeMaskWindow : Window
{
	public ThemeMaskWindow(ImageSource snapshot, Window owner)
	{
		Owner = owner;
		WindowStyle = WindowStyle.None;
		ResizeMode = ResizeMode.NoResize;
		ShowInTaskbar = false;
		Topmost = true;

		Left = owner.Left;
		Top = owner.Top;
		Width = owner.ActualWidth;
		Height = owner.ActualHeight;

		Content = new System.Windows.Controls.Image
		{
			Source = snapshot,
			Stretch = Stretch.None
		};
	}
}