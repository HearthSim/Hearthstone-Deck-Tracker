#region

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats.Converters
{
	public class ToUpperValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var str = value as string;
			return string.IsNullOrEmpty(str) ? string.Empty : str.ToUpper();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
