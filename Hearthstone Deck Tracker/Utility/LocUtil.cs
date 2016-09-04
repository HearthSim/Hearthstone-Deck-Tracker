using System.Globalization;
using WPFLocalizeExtension.Engine;

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class LocUtil
	{
		public static void UpdateCultureInfo()
		{
			var locStr = Config.Instance.Localization.ToString().Insert(2, "-");
			LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(locStr);
		}

		public static string Get(string key, bool upper = false)
		{
			var str = LocalizeDictionary.Instance.GetLocalizedObject("HearthstoneDeckTracker", "Strings", key,
				LocalizeDictionary.Instance.Culture)?.ToString();
			if(str == null)
				return string.Empty;
			return upper ? str.ToUpper(LocalizeDictionary.Instance.Culture) : str;
		}
	}
}
