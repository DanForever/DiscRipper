using System.ComponentModel;

namespace DiscRipper
{
    /// <summary>
    /// Wraps the values from the runner object into properties that are useful for WPF controls
    /// </summary>
    internal class MakeMkvRunnerViewModel : INotifyPropertyChanged
    {
        #region Public properties

        public string StandardOutput { get; private set; } = "";

        public string StandardError { get; private set; } = "";

        #endregion Public properties

        #region Public Methods

        public static MakeMkvRunnerViewModel operator +(MakeMkvRunnerViewModel runnerViewModel, MakeMkvRunner runner)
        {
            runner.StandardOutputRecieved += runnerViewModel.StandardOutputRecieved;
            runner.StandardErrorRecieved += runnerViewModel.StandardErrorRecieved;

            return runnerViewModel;
        }

        public static MakeMkvRunnerViewModel operator -(MakeMkvRunnerViewModel runnerViewModel, MakeMkvRunner runner)
        {
            runner.StandardOutputRecieved -= runnerViewModel.StandardOutputRecieved;
            runner.StandardErrorRecieved -= runnerViewModel.StandardErrorRecieved;

            return runnerViewModel;
        }

        #endregion Public Methods

        #region Event handlers

        private void StandardOutputRecieved(string output)
        {
            StandardOutput += output;
            StandardOutput += "\n";
            OnPropertyChanged(nameof(StandardOutput));
        }

        private void StandardErrorRecieved(string output)
        {
            StandardError += output;
            StandardError += "\n";
            OnPropertyChanged(nameof(StandardError));
        }

        #endregion Event handlers

        #region INotifyPropertyChanged

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion INotifyPropertyChanged
    }
}
