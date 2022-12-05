using System;
using System.Globalization;
using System.Windows.Data;

namespace Hearthstone_Deck_Tracker.Utility.Converters
{
	public class CenteredTooltipConverter : IMultiValueConverter
	{
		public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture) => (value[0], value[1]) switch
		{
			(double tooltipWidth, double targetWidth) => targetWidth / 2 - tooltipWidth / 2,
			_ => 0.0,
		};

		public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
	}
}
