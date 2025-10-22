using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using DiscRipper.Sessions;

namespace DiscRipper.Windows
{
    public partial class SessionPicker : Window
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

                SubmitNewDisc submitNewDisc = new(session, log) { Owner = this };
                submitNewDisc.Show();
            }
        }
    }
}
