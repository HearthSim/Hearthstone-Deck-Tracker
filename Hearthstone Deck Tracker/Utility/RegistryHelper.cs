using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Microsoft.Win32;

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class RegistryHelper
	{
		private const string KeyName = "Hearthstone Deck Tracker";
		private static string _executablePath = Application.ResourceAssembly.Location;
		private static string _args;
		private static RegistryKey GetRunKey() => Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

		public static void SetRunKey()
		{
			if(string.IsNullOrEmpty(_executablePath))
				return;
			var path = $"\"{_executablePath}\"";
			if(!string.IsNullOrEmpty(_args))
				path += " " + _args;
			using(var key = GetRunKey())
				key?.SetValue(KeyName, path);
			Log.Info("Set AutoRun path to " + path);
		}

		public static void DeleteRunKey()
		{
			using(var key = GetRunKey())
				key?.DeleteValue(KeyName, false);
			Log.Info("Deleted AutoRun key");
		}

		internal static void SetExecutablePath(string executablePath) => _executablePath = executablePath;
		internal static void SetExecutableArgs(string args) => _args = args;
	}
}
