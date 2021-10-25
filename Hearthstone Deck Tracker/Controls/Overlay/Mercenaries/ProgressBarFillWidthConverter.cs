using System;
using System.Globalization;
using System.Windows.Data;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Mercenaries
{
	public class ProgressBarFillWidthConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if(values.Length != 2)
				return 0.0;
			if(!(values[0] is double value))
				return 0.0;
			if(!(values[1] is double containerWidth))
				return 0.0;
			return containerWidth * value;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
	}
}
