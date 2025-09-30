using System.IO;
using System.Windows;

namespace DiscRipper
{
    internal partial class EditSettings : Window
    {
        #region C-Tor

        public EditSettings()
        {
            InitializeComponent();
        }

        #endregion C-Tor

        #region Event handlers

        private void Click_SelectMakemkvDirectory(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                Microsoft.Win32.OpenFolderDialog dialog = new()
                {
                    Multiselect = false,
                    Title = "Select the folder where MakeMkv is installed",
                    InitialDirectory = Settings.Default.MakeMkvInstallFolder,
                };

                if (dialog.ShowDialog(this) == true)
                {
                    string path = Path.Join(dialog.FolderName, "makemkvcon.exe");

                    if (File.Exists(path))
                    {
                        Settings.Default.MakeMkvInstallFolder = dialog.FolderName;
                        Settings.Default.Save();

                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void Click_SelectRepositoryDirectory(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFolderDialog dialog = new()
            {
                Multiselect = false,
                Title = "Select the folder containing your local clone of TheDiscDb repository",
                InitialDirectory = Settings.Default.MakeMkvInstallFolder,
            };

            if (dialog.ShowDialog(this) == true)
            {
                Settings.Default.RepositoryFolder = dialog.FolderName;

                Settings.Default.Save();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
        }

        #endregion Event handlers
    }
}
