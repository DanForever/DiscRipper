using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using DiscRipper.ViewModel;

namespace DiscRipper.Controls;

public partial class DiscTitles : UserControl
{
	#region C-Tor

	public DiscTitles()
	{
		InitializeComponent();

		Loaded += OnLoaded;
	}

	#endregion C-Tor

	#region Private Methods

	private void SetSeriesPropertiesVisible(bool visible)
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

	#endregion Private Methods

	#region Event Handlers

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (DataContext is Submission submission)
		{
			submission.PropertyChanged += SubmissionPropertyChanged;

			SetSeriesPropertiesVisible(submission.MediaType_New == Types.MediaTypes.Series);
		}
	}

	private void SubmissionPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if(e.PropertyName == nameof(Submission.MediaType) || e.PropertyName == nameof(Submission.MediaType_New))
		{
			Submission submission = (Submission)DataContext;

			SetSeriesPropertiesVisible(submission.MediaType_New == Types.MediaTypes.Series);
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

	#endregion Event Handlers
}
