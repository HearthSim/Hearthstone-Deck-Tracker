using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker.Utility.Converters
{
	public class GameStatsHasReplayConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var game = value as GameStats;
			if(game == null)
				return DependencyProperty.UnsetValue;
			return game.HsReplay.Uploaded && !game.HsReplay.Unsupported || game.HasReplayFile;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
