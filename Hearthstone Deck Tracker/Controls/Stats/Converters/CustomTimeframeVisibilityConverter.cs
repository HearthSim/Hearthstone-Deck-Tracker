#region

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats.Converters
{
	class CustomTimeframeVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null)
				return Visibility.Collapsed;
			DisplayedTimeFrame timeFrame;
			if(Enum.TryParse(value.ToString(), out timeFrame))
				return timeFrame == DisplayedTimeFrame.Custom ? Visibility.Visible : Visibility.Collapsed;
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
