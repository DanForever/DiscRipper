using System.Windows;
using System.Windows.Controls;

namespace DiscRipper.Controls;

public partial class Heading : UserControl
{
	public string Text
	{
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(Heading));

	public Heading()
	{
		InitializeComponent();
	}
}
