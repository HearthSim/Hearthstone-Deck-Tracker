#region

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats.Converters
{
	class NotNullToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
