using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.Controls.Stats.Converters
{
	public class CustomTimeframeSeasonVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null)
				return Visibility.Collapsed;
		    if(Enum.TryParse<DisplayedTimeFrame>(value.ToString(), out var timeFrame))
				return timeFrame == DisplayedTimeFrame.CustomSeason ? Visibility.Visible : Visibility.Collapsed;
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
