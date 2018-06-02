#region

using System;
using System.Globalization;
using System.Windows.Data;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker
{
	public class DateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is DateTime))
				return string.Empty;
			var date = (DateTime)value;
			if(date == DateTime.MinValue)
				return "N/A";
			return date.ToString(EnumDescriptionConverter.GetDescription(Config.Instance.SelectedDateFormat), culture);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
	}
}
