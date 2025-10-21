using System.Windows;

using DiscRipper.Sessions;

namespace DiscRipper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await SessionManager.Instance.Value.LoadAsync();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            SessionManager.Instance.Value.Save();
        }
    }

}
