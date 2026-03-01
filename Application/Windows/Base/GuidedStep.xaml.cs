
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

using DiscRipper.Sessions;
using DiscRipper.ViewModel;

namespace DiscRipper.Windows.Base;

[ContentProperty("InnerContent")]
internal partial class GuidedStep
{
	public static readonly DependencyProperty StepNumberProperty = DependencyProperty.Register
	(
		nameof(StepNumber),
		typeof(string),
		typeof(GuidedStep),
		new PropertyMetadata("Step number not set")
	);

	public static readonly DependencyProperty TitleProperty = DependencyProperty.Register
	(
		nameof(Title),
		typeof(string),
		typeof(GuidedStep),
		new PropertyMetadata("Title not set")
	);

	public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register
	(
		nameof(Description),
		typeof(string),
		typeof(GuidedStep),
		new PropertyMetadata("Description not set")
	);

	public static readonly DependencyProperty StepNumberIconProperty = DependencyProperty.Register("StepNumberIcon", typeof(ControlTemplate), typeof(GuidedStep));
	public static readonly DependencyProperty InnerContentProperty = DependencyProperty.Register("InnerContent", typeof(object), typeof(GuidedStep));
	public static readonly DependencyProperty HasPreviousProperty = DependencyProperty.Register("HasPrevious", typeof(bool), typeof(GuidedStep));

	public string StepNumber
	{
		get => (string)GetValue(StepNumberProperty);
		set => SetValue(StepNumberProperty, value);
	}
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

	public ControlTemplate StepNumberIcon
	{
		get { return (ControlTemplate)GetValue(StepNumberIconProperty); }
		set { SetValue(StepNumberIconProperty, value); }
	}

	public object InnerContent
	{
		get { return (object)GetValue(InnerContentProperty); }
		set { SetValue(InnerContentProperty, value); }
	}

	public bool HasPrevious
	{
		get { return (bool)GetValue(HasPreviousProperty); }
		set { SetValue(HasPreviousProperty, value); }
	}

	public Submission Submission { get; init; }
	public Session Session { get; init; }
	public MakeMkv.Log Log { get; init; }

	public GuidedStep()
	{
		InitializeComponent();
	}

	public event RoutedEventHandler NextClicked
	{
		add => Next.Click += value;
		remove => Next.Click -= value;
	}

	public event RoutedEventHandler PreviousClicked
	{
		add => Previous.Click += value;
		remove => Previous.Click -= value;
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
