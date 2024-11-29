using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Hearthstone_Deck_Tracker.Utility.Converters
{
	internal static class ConverterHelper
	{
		/// <summary>
		/// Takes an array of booleans as values, and an optional array of booleans as parameters.
		/// A `true` parameter value will signal the value at the same index to be inverted:
		/// e.g. values = [true, true] and parameters = [true, false] will invert the first value => [false, true]
		///
		/// This will return true if all values, after potentially being inverted, are true.
		/// </summary>
		/// <param name="values">Array of booleans</param>
		/// <param name="parameter">Array of flags indicating whether the value should be inverted</param>
		/// <returns>True if all values are true (accounting for invert flags). False otherwise.</returns>
		public static bool BoolConverter(object[] values, object parameter)
		{
			var paramBools = parameter as IEnumerable<bool>;
			var invertFlags = values.Select((_, i) => paramBools?.ElementAtOrDefault(i) ?? false).ToArray();
			for(var i = 0; i < values.Length; i++)
			{
				if(values[i] is not bool b)
					return false;
				if(invertFlags[i])
					b = !b;
				if(!b)
					return false;
			}
			return true;
		}
	}

	public class BoolToVisibilityConverter : IValueConverter, IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var hidden = parameter is Visibility v ? v : Visibility.Collapsed;
			return ConverterHelper.BoolConverter(values, parameter) ? Visibility.Visible : hidden;
		}

		public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(new[] { value }, targetType, parameter, culture);
		}

		public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}

	public class InverseBoolToVisibilityConverter : IValueConverter, IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var hidden = parameter is Visibility v ? v : Visibility.Collapsed;
			return ConverterHelper.BoolConverter(values, parameter) ? hidden : Visibility.Visible;
		}

		public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(new[] { value }, targetType, parameter, culture);
		}

		public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
