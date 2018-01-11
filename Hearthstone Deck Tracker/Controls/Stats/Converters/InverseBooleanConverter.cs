using System;
using System.Globalization;
using System.Windows.Data;

namespace Hearthstone_Deck_Tracker.Controls.Stats.Converters
{
	public class InverseBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		  return targetType != typeof(bool) ? (object) null : value != null && !(bool) value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		  return targetType != typeof(bool) ? (object) null : value != null && !(bool) value;
		}
	}
}
