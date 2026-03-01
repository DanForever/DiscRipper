using System.Windows;
using System.Windows.Input;

namespace DiscRipper.Utilities;

internal static class FocusHelper
{
	public static bool GetAutoFocus(DependencyObject obj) => (bool)obj.GetValue(AutoFocusProperty);

	public static void SetAutoFocus(DependencyObject obj, bool value) => obj.SetValue(AutoFocusProperty, value);

	public static readonly DependencyProperty AutoFocusProperty =
		DependencyProperty.RegisterAttached
		(
			"AutoFocus",
			typeof(bool),
			typeof(FocusHelper),
			new PropertyMetadata(false, OnAutoFocusChanged)
		);

	private static void OnAutoFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FrameworkElement element && (bool)e.NewValue)
		{
			element.Loaded += OnLoaded;
		}
	}
	private static void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement element)
		{
			element.Loaded -= OnLoaded; // prevent repeated firing
			element.Focus();
			Keyboard.Focus(element);
		}
	}
}
