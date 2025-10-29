using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DiscRipper.Converters
{
	/// <summary>
	/// A WPF converter that takes a string and returns Visibility.Collapsed
	/// if the string matches the 'MatchString' property.
	/// Otherwise, it returns Visibility.Visible.
	/// </summary>
	internal class StringToCollapsedConverter : IValueConverter
	{
		/// <summary>
		/// Gets or sets the string value that will trigger the converter
		/// to return Visibility.Collapsed.
		/// This can be set from XAML.
		/// </summary>
		public string MatchString { get; set; }

		/// <summary>
		/// Converts a string to a Visibility value.
		/// </summary>
		/// <param name="value">The string value from the binding source.</param>
		/// <param name="targetType">The type of the binding target property (expected to be Visibility).</param>
		/// <param name="parameter">The converter parameter (not used).</param>
		/// <param name="culture">The culture to use in the converter (not used).</param>
		/// <returns>Visibility.Collapsed if the value matches MatchString; otherwise, Visibility.Visible.</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// Check if the input is a string and a MatchString has been provided
			if (value is string inputString && !string.IsNullOrEmpty(MatchString))
			{
				// Compare the string to the public MatchString property
				if (inputString == this.MatchString)
				{
					// If it matches, return Collapsed
					return Visibility.Collapsed;
				}
			}

			// For any other value (including null, non-strings, or non-matches),
			// return Visible so the control shows by default.
			return Visibility.Visible;
		}

		/// <summary>
		/// Converts a Visibility value back to a string. This is not supported.
		/// </summary>
		/// <param name="value">The value from the binding target (Visibility).</param>
		/// <param name="targetType">The type to convert to (not used).</param>
		/// <param name="parameter">The converter parameter (not used).</param>
		/// <param name="culture">The culture to use in the converter (not used).</param>
		/// <returns>Always returns Binding.DoNothing as this is a one-way converter.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// This converter is designed to be one-way (string -> Visibility).
			// We don't need to convert back from Visibility to string.
			// Binding.DoNothing tells the binding engine to ignore this.
			return Binding.DoNothing;
		}
	}
}
