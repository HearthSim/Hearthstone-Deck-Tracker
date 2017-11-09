using System;
using System.Globalization;
using System.Windows.Data;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using static System.Windows.Visibility;
using Type = System.Type;

namespace Hearthstone_Deck_Tracker.Controls.DeckSetIcons
{
	public class SetIconVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is Deck deck))
				return Collapsed;
			if(!Enum.TryParse(parameter?.ToString(), out CardSet set))
				return Collapsed;
			return deck.ContainsSet(set) ? Visible : Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
