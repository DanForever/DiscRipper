using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

using DiscRipper.TheDiscDb.Repo;

namespace DiscRipper.Windows;

public partial class ReleasePicker : Window
{
	private string _filterText;

	public string RepositoryDataFolder => Path.Join(Settings.Default.RepositoryFolder, "data");

	public List<DiscRipper.Types.Media> Media { get; private init; } = new();

	public ICollectionView FilteredMedia { get; private init; }

	public Types.Release? Release { get; private set; }

	public string FilterText
	{
		get => _filterText;
		set
		{
			_filterText = value;
			FilteredMedia.Refresh();
		}
	}

	public ReleasePicker()
	{
		InitializeComponent();

		ReleaseScanner scanner = new();
		Media = scanner.Scan(Settings.Default.RepositoryFolder);

		FilteredMedia = CollectionViewSource.GetDefaultView(Media);
		FilteredMedia.Filter = OnFilter;

		DataContext = this;
	}

	private bool OnFilter(object item)
	{
		if (string.IsNullOrEmpty(FilterText))
			return true;

		var media = item as DiscRipper.Types.Media;
		if (media == null)
			return false;

		string filter = FilterText.ToLower();

		if (media.Title.ToLower().Contains(filter))
			return true;

		if (media.Releases.Any(release => release.Slug.ToLower().Contains(filter)))
			return true;

		return false;
	}

	private void Tree_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		var clickedElement = e.OriginalSource as FrameworkElement;
		if (clickedElement == null)
			return;

		var dataItem = clickedElement.DataContext;

		if (dataItem is DiscRipper.Types.Release release)
		{
			Release = release;

			this.Close();
		}
	}
}
