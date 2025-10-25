using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DiscRipper.ViewModel;


public class ViewModel : INotifyPropertyChanged
{
    #region PropertyChanged Wrappers

    // View models should use this method to change properties on their models, and not set them directly.
    protected void ChangeProperty<TModel, TValue>(TModel model, TValue value, [CallerMemberName] string propertyName = "")
    {
        PropertyInfo? prop = typeof(TModel).GetProperty(propertyName);

        if (prop == null)
            return;

        var oldValue = prop.GetValue(model);

        if (!Equals(oldValue, value))
        {
            prop.SetValue(model, value);
            AnnouncePropertyChanged(propertyName);
        }
    }

    protected void AnnouncePropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion PropertyChanged Wrappers

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion
}
