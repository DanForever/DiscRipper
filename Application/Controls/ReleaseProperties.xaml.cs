using System.Windows.Controls;

namespace DiscRipper.Controls
{
	public partial class ReleaseProperties : UserControl
	{
		public ReleaseProperties()
		{
			InitializeComponent();
		}

		public event System.Windows.Controls.SelectionChangedEventHandler? MediaTypeChanged;

		private void OnMediaTypeChanged(object sender, SelectionChangedEventArgs e)
		{
			MediaTypeChanged?.Invoke(sender, e);
		}
	}
}
