
using System.Windows;
using System.Windows.Markup;

using DiscRipper.Sessions;
using DiscRipper.ViewModel;

namespace DiscRipper.Windows.Base;

[ContentProperty("InnerContent")]
internal partial class GuidedStep
{
	public static readonly DependencyProperty TitleProperty =
		DependencyProperty.Register
		(
			nameof(Title),
			typeof(string),
			typeof(GuidedStep),
			new PropertyMetadata("Title not set")
		);

	public static readonly DependencyProperty DescriptionProperty =
		DependencyProperty.Register
		(
			nameof(Description),
			typeof(string),
			typeof(GuidedStep),
			new PropertyMetadata("Description not set")
		);

	public static readonly DependencyProperty InnerContentProperty = DependencyProperty.Register("InnerContent", typeof(object), typeof(GuidedStep));

	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public string Description
	{
		get => (string)GetValue(DescriptionProperty);
		set => SetValue(DescriptionProperty, value);
	}

	public object InnerContent
	{
		get { return (object)GetValue(InnerContentProperty); }
		set { SetValue(InnerContentProperty, value); }
	}

	public Submission Submission { get; init; }
	public Session Session { get; init; }
	public MakeMkv.Log Log { get; init; }

	public GuidedStep()
	{
		InitializeComponent();
	}


	private void Next_Click(object sender, RoutedEventArgs e)
	{

	}

	private void Previous_Click(object sender, RoutedEventArgs e)
	{

	}

	private void AdvancedMode_Click(object sender, RoutedEventArgs e)
	{
		SubmitRelease submitNewDisc = new(Submission, Session, Log) { Owner = Owner };
		submitNewDisc.Show();
		Close();
	}

	private void Cancel_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
