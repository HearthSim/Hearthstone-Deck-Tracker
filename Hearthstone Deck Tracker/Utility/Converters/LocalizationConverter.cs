using System;
using System.Globalization;
using System.Windows.Data;

namespace Hearthstone_Deck_Tracker.Utility.Converters
{
	public class LocalizationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(string.IsNullOrEmpty(value as string))
				return value;
			return LocUtil.Get((string) value) ?? value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
