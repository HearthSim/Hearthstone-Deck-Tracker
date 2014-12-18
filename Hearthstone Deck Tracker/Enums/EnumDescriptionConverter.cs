using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Enums
{
	public class EnumDescriptionConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null)
				return DependencyProperty.UnsetValue;
			return GetDescription((Enum)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Enum.ToObject(targetType, value);
		}

		public string GetDescription(Enum en)
		{
			Type type = en.GetType();
			MemberInfo[] memInfo = type.GetMember(en.ToString());
			if(memInfo.Length > 0)
			{
				object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
				if(attrs.Length > 0)
				{
					return Lang.GetLocalizedString(((DescriptionAttribute)attrs[0]).Description);
				}
			}
			return en.ToString();
		}
	}
}
