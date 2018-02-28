using System;
using System.Collections.Generic;
using System.Globalization;
using WPFLocalizeExtension.Engine;

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class LocUtil
	{
		private const string LocAgeDay = "GameStats_Age_Day";
		private const string LocAgeDays = "GameStats_Age_Days";
		private const string LocAgeHour = "GameStats_Age_Hour";
		private const string LocAgeHours = "GameStats_Age_Hours";
		private const string LocAgeMinute = "GameStats_Age_Minute";
		private const string LocAgeMinutes = "GameStats_Age_Minutes";

		private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>();

		public static void UpdateCultureInfo()
		{
			var locStr = Config.Instance.Localization.ToString();
			if(locStr.Length > 2)
				locStr = locStr.Insert(2, "-");
			LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(locStr);
		}

		public static string Get(string key, bool upper = false)
		{
			var culture = LocalizeDictionary.Instance.Culture;
			var cacheKey = culture + key;
			if(!Cache.TryGetValue(cacheKey, out var str))
			{
				str = LocalizeDictionary.Instance.GetLocalizedObject("HearthstoneDeckTracker", "Strings", key, culture)?.ToString();
				Cache[cacheKey] = str;
			}
			return str != null && upper ? str.ToUpper(culture) : str;
		}

		public static string GetAge(DateTime since)
		{
			var duration = DateTime.Now - since;
			int time;
			string str;
			if(duration.TotalDays >= 2)
			{
				str = LocAgeDays;
				time = (int)duration.TotalDays;
			}
			else if(duration.TotalDays >= 1)
			{
				str = LocAgeDay;
				time = (int)duration.TotalDays;
			}
			else if(duration.TotalHours >= 2)
			{
				str = LocAgeHours;
				time = (int)duration.TotalHours;
			}
			else if(duration.TotalHours >= 1)
			{
				str = LocAgeHour;
				time = (int)duration.TotalHours;
			}
			else if(duration.TotalMinutes >= 2 || duration.TotalMinutes < 1)
			{
				str = LocAgeMinutes;
				time = (int)duration.TotalMinutes;
			}
			else
			{
				str = LocAgeMinute;
				time = (int)duration.TotalMinutes;
			}
			return string.Format(Get(str), time);
		}
	}
}
