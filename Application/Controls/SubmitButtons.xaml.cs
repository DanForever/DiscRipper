using System.Windows;
using System.Windows.Controls;

namespace DiscRipper.Controls;

internal partial class SubmitButtons : UserControl
{
	#region Dependency Properties

	public static readonly DependencyProperty GuidedViewVisibleProperty =
		DependencyProperty.Register(
			nameof(GuidedViewButton),
			typeof(bool),
			typeof(SubmitButtons),
			new PropertyMetadata(false));

	#endregion Dependency Properties

	#region Public Properties

	public bool GuidedViewVisible
	{
		get => (bool)GetValue(GuidedViewVisibleProperty);
		set => SetValue(GuidedViewVisibleProperty, value);
	}

	#endregion Public Properties

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

	public event RoutedEventHandler SwitchToGuidedView
	{
		add
		{
			GuidedViewButton.Click += value;
			GuidedViewVisible = true;
		}

		remove => GuidedViewButton.Click -= value;
	}

	#endregion Events
}
