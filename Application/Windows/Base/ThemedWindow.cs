using System;
using System.Windows.Media;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DiscRipper.Windows.Base;

internal class ThemedWindow : FluentWindow
{

	protected override void OnSourceInitialized(EventArgs e)
	{
		base.OnSourceInitialized(e);

		Wpf.Ui.Appearance.ApplicationTheme currentAppTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme();
		Wpf.Ui.Appearance.WindowBackgroundManager.UpdateBackground(this, currentAppTheme, Wpf.Ui.Controls.WindowBackdropType.None);

		Wpf.Ui.Appearance.ApplicationThemeManager.Changed += OnThemeChanged;
	}

	private void OnThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
	{
		Wpf.Ui.Appearance.WindowBackgroundManager.UpdateBackground(this, currentApplicationTheme, Wpf.Ui.Controls.WindowBackdropType.None);
	}
}
