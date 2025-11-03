using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiscRipper.Controls;

/// <summary>
/// Interaction logic for DiscTitles.xaml
/// </summary>
public partial class DiscTitles : UserControl
{
	public DiscTitles()
	{
		InitializeComponent();
	}

	public void SetSeriesPropertiesVisible(bool visible)
	{
		var seasonColumn = TitleDetails.Columns.First(c => c.Header?.ToString() == "Season");
		var episodeColumn = TitleDetails.Columns.First(c => c.Header?.ToString() == "Episode");

		if (visible)
		{
			seasonColumn.Visibility = Visibility.Visible;
			episodeColumn.Visibility = Visibility.Visible;
		}
		else
		{
			seasonColumn.Visibility = Visibility.Collapsed;
			episodeColumn.Visibility = Visibility.Collapsed;
		}
	}

	private void Title_Selected(object sender, SelectionChangedEventArgs e)
	{
		var grid = (DataGrid)sender;
		if (grid.SelectedItem != null)
		{
			SelectedTitleDetails.DataContext = grid.SelectedItem;
		}
	}
}
