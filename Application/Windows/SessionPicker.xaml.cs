using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using DiscRipper.Sessions;

namespace DiscRipper.Windows;

internal partial class SessionPicker
{
	public SessionPicker()
	{
		InitializeComponent();

		DataContext = SessionManager.Instance.Value.SessionList;
	}

	private async void Session_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;
		if (row != null && row.Item is Session session)
		{
			MakeMkv.Log? log = await SessionManager.Instance.Value.LoadLog(session);

			if (log is null)
				return;

			if (session.Release is not null)
			{
				SubmitRelease submitNewDisc = new(session, log) { Owner = this };
				submitNewDisc.Show();
			}
			else
			{
				SubmitAdditionalDisc submitSecondDisc = new(session, log) { Owner = this };
				submitSecondDisc.Show();
				return;
			}
		}
	}
}
