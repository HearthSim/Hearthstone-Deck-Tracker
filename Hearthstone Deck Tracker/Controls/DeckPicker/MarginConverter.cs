#region

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker
{
	public class MarginConverterNegativeLeft : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new Thickness(-System.Convert.ToDouble(value), 0, 0, 0);

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
	}

	public class MarginConverterRight : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new Thickness(0, 0, System.Convert.ToDouble(value) + 8, 0);

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
	}
}
