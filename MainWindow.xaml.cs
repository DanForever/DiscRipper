using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DiscRipper
{
    public partial class MainWindow : Window
    {
        #region Private fields

        private readonly Scanner _ripper = new();
        private readonly TheDiscDb.Querier _querier = new();
        private readonly TitleMapper _titleMapper = new();

        #endregion Private fields

        #region C-Tor

        public MainWindow()
        {
            InitializeComponent();

            DataContext = _ripper;
            Loaded += MainWindow_Loaded;
        }

        #endregion C-Tor

        #region Event handlers

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
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

        #endregion Event handlers

        #region Private methods

        private async Task ScanDrives()
        {
            string output = await RipperUtils.ScanDrives(Feedback);

            var drives = Scanner.AnalyseDriveData(output);
            foreach (var item in drives)
                _ripper.Drives.Add(item);
        }

        private async Task ScanDisc(MakeMkv.Drive drive)
        {
            string output = await RipperUtils.ScanDisc(Feedback, drive);

            var titles = Scanner.AnalyseTitleData(output);
            var longestTitle = Scanner.GetLongestTitle(titles);

            await _querier.Query(longestTitle.Duration);

            List<Mapped.Disc> mappedDiscs = TitleMapper.Map(titles, _querier.Nodes);

            if (mappedDiscs.Count > 0)
            {
                BestMatch bestMatch = new(mappedDiscs, drive) { Owner = this };
                bestMatch.Show();
            }
        }

        private async void TestQueryAvatar()
        {
            await TestQuery(DummyData.Avatar);
        }

        private async void TestQueryLayerCake()
        {
            await TestQuery(DummyData.LayerCake);
        }

        private async Task TestQuery(string data)
        {
            var titles = Scanner.AnalyseTitleData(data);
            var longestTitle = Scanner.GetLongestTitle(titles);

            await _querier.Query(longestTitle.Duration);

            List<Mapped.Disc> mappedDiscs = TitleMapper.Map(titles, _querier.Nodes);

            if (mappedDiscs.Count > 0)
            {
                BestMatch bestMatch = new(mappedDiscs, null);
                bestMatch.ShowDialog();
            }
        }

        #endregion Private methods
    }
}
