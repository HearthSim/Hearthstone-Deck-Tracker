﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Hearthstone_Deck_Tracker.Controls.Stats.Converters
{
	public class CardImageSizeBooleanConverter : IValueConverter
	{
		public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType != typeof(bool))
				return null;
			return (double)value <= 1;
		}

		public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
