#region

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker
{
	public class DeckTypeIconConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null || !Enum.TryParse(value.ToString(), out DeckType deckType))
				return null;
			var resource = "mode_" + deckType.ToString().ToLowerInvariant();
			return Core.MainWindow.DeckPickerList.TryFindResource(resource) as Canvas;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
