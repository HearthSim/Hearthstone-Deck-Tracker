using System;
using System.Globalization;
using System.Windows.Data;

namespace Hearthstone_Deck_Tracker.Utility.Converters;

public class AddConverter : IValueConverter
{
	public double Value { get; set; } = 7;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
			return 0;

		try
		{
			var x = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
			return x + Value;
		}
		catch
		{
			return 0;
		}
	}

	public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> null;
}
