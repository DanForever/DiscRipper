using System.Windows;
using System.Windows.Controls;

namespace DiscRipper
{
    internal partial class MakeMkvFeedback : Window
    {
        #region C-Tor

        public MakeMkvFeedback()
        {
            InitializeComponent();
        }

        #endregion C-Tor

        #region Public methods

        public static MakeMkvFeedback operator +(MakeMkvFeedback feedback, MakeMkvRunner runner)
        {
            feedback.Feedback += runner;
            return feedback;
        }

        public static MakeMkvFeedback operator -(MakeMkvFeedback feedback, MakeMkvRunner runner)
        {
            feedback.Feedback -= runner;
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
