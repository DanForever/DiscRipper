using System.Windows;
using System.Windows.Controls;

namespace DiscRipper.Controls;

internal partial class SubmitButtons : UserControl
{
	#region C-Tor

	public SubmitButtons()
	{
		InitializeComponent();
	}

	#endregion C-Tor

	#region Events

	public event RoutedEventHandler Submit
	{
		add => SubmitButton.Click += value;
		remove => SubmitButton.Click -= value;
	}

	public event RoutedEventHandler Cancel
	{
		add => CancelButton.Click += value;
		remove => CancelButton.Click -= value;
	}

	#endregion Events
}
