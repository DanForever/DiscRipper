using System.Windows;

using DiscRipper.Sessions;
using DiscRipper.Windows;

namespace DiscRipper
{
	public partial class App : Application
	{
		private async void OnStartup(object sender, StartupEventArgs e)
		{
			var accentColour = System.Windows.Media.Color.FromArgb(0xFF, 0xEE, 0x00, 0xBB);

			Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Light, Wpf.Ui.Controls.WindowBackdropType.None);
			Wpf.Ui.Appearance.ApplicationAccentColorManager.Apply(accentColour, Wpf.Ui.Appearance.ApplicationTheme.Light);

			await SessionManager.Instance.Value.LoadAsync();

			MainWindow window = new MainWindow();
			MainWindow = window;
			window.Show();

			Wpf.Ui.Appearance.WindowBackgroundManager.UpdateBackground(window,Wpf.Ui.Appearance.ApplicationTheme.Light, Wpf.Ui.Controls.WindowBackdropType.None);
		}

		private void OnExit(object sender, ExitEventArgs e)
		{
			SessionManager.Instance.Value.Save();
		}

		private void Application_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{

		}
	}
}
