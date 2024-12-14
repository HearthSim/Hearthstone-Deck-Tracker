using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hearthstone_Deck_Tracker.Utility.Converters;

public class NullableToVisibilityConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var hidden = parameter is Visibility v ? v : Visibility.Collapsed;
		return value is null ? hidden : Visibility.Visible;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}
