using System.Collections.Generic;
using System.ComponentModel;

namespace DiscRipper
{
    /// <summary>
    /// Presents a more useful api to WPF for mapped titles
    /// </summary>
    internal class MappedTitleViewModel : INotifyPropertyChanged
    {
        #region Private fields

        private readonly Mapped.Title _mappedTitle;

        private bool _rip = true;

        #endregion Private fields

        #region Public properties

        public bool Rip
        {
            get => _rip;
            set
            {
                _rip = value;
                NotifyPropertyChanged(nameof(Rip));
            }
        }

        public string Title => _mappedTitle.tddbTitle.Item.Title;
        public string Duration => _mappedTitle.mmkvTitle.Duration;

        public Mapped.Title MappedTitle => _mappedTitle;

        #endregion Public properties

        #region C-Tor

        public MappedTitleViewModel(Mapped.Title mappedTitle)
        {
            _mappedTitle = mappedTitle;
        }

        #endregion C-Tor

        #region INotifyPropertyChanged

        private void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion INotifyPropertyChanged
    }

    internal class MappedTitleViewModelList : List<MappedTitleViewModel>
    {
        #region C-Tor

        public MappedTitleViewModelList(IEnumerable<Mapped.Title> mappedTitles)
        {
            foreach(Mapped.Title mappedTitle in mappedTitles)
            {
                Add(new MappedTitleViewModel(mappedTitle));
            }
        }
        
        #endregion C-Tor
    }
}
