using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Utility.Converters
{
	public class EnumToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is not Enum enumValue || (value == null && parameter == null))
			{
				return Visibility.Collapsed;
			}
			return enumValue?.ToString() == parameter?.ToString() ? Visibility.Visible : Visibility.Collapsed;
		}

		public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
	}
}
