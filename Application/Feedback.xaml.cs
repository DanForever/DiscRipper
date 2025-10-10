using System.Windows.Controls;

namespace DiscRipper
{
    /// <summary>
    /// Displays Makemkv progress back to the user, so that they know the program is doing something
    /// </summary>
    internal partial class Feedback : UserControl
    {
        #region Private fields

        private MakeMkvRunnerViewModel _runnerViewModel = new();

        #endregion Private fields

        #region C-Tor

        public Feedback()
        {
            InitializeComponent();
            DataContext = _runnerViewModel;
        }

        #endregion C-Tor

        #region Public methods

        public static Feedback operator +(Feedback feedback, MakeMkv.Log log)
        {
            feedback._runnerViewModel += log;

            return feedback;
        }

        public static Feedback operator -(Feedback feedback, MakeMkv.Log log)
        {
            feedback._runnerViewModel -= log;

            return feedback;
        }

        #endregion Public methods

        #region Event Handlers

        private void StdOut_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).ScrollToEnd();
        }

        #endregion Event Handlers
    }
}
