using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DiscRipper
{
    [ContentProperty("InnerContent")]
    public partial class Setting : UserControl
    {
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(Setting),
                new PropertyMetadata(string.Empty));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty InnerContentProperty =
            DependencyProperty.Register("InnerContent", typeof(object), typeof(Setting));

        public object InnerContent
        {
            get { return (object)GetValue(InnerContentProperty); }
            set { SetValue(InnerContentProperty, value); }
        }

        public Setting()
        {
            InitializeComponent();
        }
    }
}
