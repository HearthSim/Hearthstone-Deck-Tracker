using System;
using System.Globalization;
using System.Windows.Data;

namespace Hearthstone_Deck_Tracker.Controls
{
	public enum CardImageSizeType
	{
		Width,
		Height
	}
	public class CardImageSizeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType != typeof(double))
				return null;
			if(!(parameter is CardImageSizeType))
				return null;
			var size = (CardImageSizeType)parameter == CardImageSizeType.Width ? 256 : 388;
			return (double)value * size;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
