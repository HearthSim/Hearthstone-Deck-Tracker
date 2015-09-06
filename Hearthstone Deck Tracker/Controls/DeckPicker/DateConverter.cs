#region

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker
{
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
            {
                var date = (DateTime)value;

                if (date == DateTime.MinValue)
                {
                    return "N/A";
                }
                return date.ToString("d", culture);
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}