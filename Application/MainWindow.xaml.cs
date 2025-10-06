using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DiscRipper
{
    public partial class MainWindow : Window
    {
        #region Private fields

        private readonly Scanner _ripper = new();
        private readonly TheDiscDb2.Querier _querier = new();
        private readonly TitleMapper _titleMapper = new();

        #endregion Private fields

        #region C-Tor

        public MainWindow()
        {
            InitializeComponent();

            //TextMakeMkv();

            DataContext = _ripper;
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
                await ScanDisc2(drive);
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
                _ripper.Drives.Add(item);
        }

        private async Task ScanDisc2(MakeMkv.Drive drive)
        {
            MakeMkv.Runner runner = new()
            {
                MakeMkvDir = Settings.Default.MakeMkvInstallFolder,
                SynchronizationContext = SynchronizationContext.Current!,
            };

            Feedback += runner.Log;
            await runner.Info(drive.Index);

            MakeMkv.TitleEngine titleEngine = new();
            await titleEngine.Read(runner.Log);

            await _querier.Query(titleEngine.LongestTitle?.Duration);

            List<Mapped.Disc> mappedDiscs = TitleMapper.Map(titleEngine.Titles.ToList(), _querier.Nodes);

            SubmitNewDisc submitNewDisc = new(runner.Log, titleEngine.Titles, drive) { Owner = this };

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
            await runner.RunDebugSimulateCallback(DummyData.TellNoOne);

            MakeMkv.TitleEngine titleEngine = new();
            await titleEngine.Read(runner.Log);

            List<Mapped.Disc> mappedDiscs = TitleMapper.Map(titleEngine.Titles.ToList(), _querier.Nodes);
            SubmitNewDisc submitNewDisc = new(runner.Log, titleEngine.Titles, null) { Owner = this };

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

        //private async Task ScanDisc(MakeMkv.Drive drive)
        //{
        //    string output = await RipperUtils.ScanDisc(Feedback, drive);

        //    var titles = Scanner.AnalyseTitleData(output);
        //    var longestTitle = Scanner.GetLongestTitle(titles);

        //    await _querier.Query(longestTitle.Duration);

        //    List<Mapped.Disc> mappedDiscs = TitleMapper.Map(titles, _querier.Nodes);

        //    if (mappedDiscs.Count > 0)
        //    {
        //        BestMatch bestMatch = new(mappedDiscs, drive) { Owner = this };
        //        bestMatch.Show();
        //    }
        //}

        private async void TextMakeMkv()
        {
            MakeMkv.Runner runner = new()
            {
                MakeMkvDir = Settings.Default.MakeMkvInstallFolder,
                SynchronizationContext = SynchronizationContext.Current!,
            };

            //await runner.RunDebug(DummyData.Avatar);
            await runner.RunDebugSimulateCallback(DummyData.Avatar);
        }

        #endregion Private methods
    }
}
