using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DiscRipper.Utilities;

public static class TextFormatter
{
	public static readonly DependencyProperty FormattedTextProperty =
		DependencyProperty.RegisterAttached(
			"FormattedText",
			typeof(string),
			typeof(TextFormatter),
			new PropertyMetadata(string.Empty, OnFormattedTextChanged));

	public static string GetFormattedText(DependencyObject obj)
	{
		return (string)obj.GetValue(FormattedTextProperty);
	}

	public static void SetFormattedText(DependencyObject obj, string value)
	{
		obj.SetValue(FormattedTextProperty, value);
	}

	private static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is TextBlock textBlock)
		{
			string text = e.NewValue as string;
			textBlock.Inlines.Clear();

			if (string.IsNullOrEmpty(text))
				return;

			// Split text by <b>...</b> tags. 
			// The regex captures the tags so we can identify them in the loop.
			string[] parts = Regex.Split(text, @"(<b>.*?</b>)");

			foreach (string part in parts)
			{
				if (part.StartsWith("<b>") && part.EndsWith("</b>"))
				{
					// Remove the tags and make this part Bold
					var cleanText = part.Substring(3, part.Length - 7);
					textBlock.Inlines.Add(new Run(cleanText) { FontWeight = FontWeights.Bold });
				}
				else
				{
					// Add normal text
					// Note: If you want <br/> support, you could handle it here too.
					// For now, it respects standard \r\n newlines.
					textBlock.Inlines.Add(new Run(part));
				}
			}
		}
	}
}