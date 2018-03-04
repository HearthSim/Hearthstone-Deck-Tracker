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
		public static bool BoolConverter(object[] values, object parameter)
		{
			if(values == null)
				return false;

			bool GetValue(object obj, bool b = false)
			{
				var val = obj as bool? ?? false;
				return b ? !val : val;
			}

			var parameters = (parameter as IEnumerable<bool> ?? new List<bool>())
				.Concat(values.Select(x => false));

			return values.Zip(parameters, GetValue).All(x => x);
		}
	}

	public class BoolToVisibilityConverter : IValueConverter, IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			return ConverterHelper.BoolConverter(values, parameter) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(new[] { value }, targetType, parameter, culture);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}

	public class InverseBoolToVisibilityConverter : IValueConverter, IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			return ConverterHelper.BoolConverter(values, parameter) ? Visibility.Collapsed : Visibility.Visible;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(new[] { value }, targetType, parameter, culture);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
