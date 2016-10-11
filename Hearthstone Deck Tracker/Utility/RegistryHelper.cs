using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Microsoft.Win32;

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class RegistryHelper
	{
		private const string KeyName = "Hearthstone Deck Tracker";
		private static RegistryKey GetRunKey() => Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

		public static void SetRunKey()
		{
			var location = Application.ResourceAssembly.Location;
			if(string.IsNullOrEmpty(location))
				return;
			using(var key = GetRunKey())
				key?.SetValue(KeyName, $"\"{location}\"");
			Log.Info("Set AutoRun path to " + location);
		}

		public static void DeleteRunKey()
		{
			using(var key = GetRunKey())
				key?.DeleteValue(KeyName, false);
			Log.Info("Deleted AutoRun key");
		}
	}
}
