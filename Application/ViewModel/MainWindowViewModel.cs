using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscRipper.ViewModel;

internal class MainWindowViewModel
{
    public ObservableCollection<MakeMkv.Drive> Drives { get; set; } = [];
}
