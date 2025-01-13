using System;
using System.Globalization;
using System.Windows.Data;

namespace Hearthstone_Deck_Tracker.Utility.Converters;

public class ObjectTypeConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return value?.GetType();
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}
