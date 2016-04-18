#region

using System.IO;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal class Constants
	{
		private const string Msvcr100Dll = "msvcr100.dll";
		private const string HsReplayDir = "HsReplay";
		private const string HsReplayExeFilename = "hsreplayconverter.exe";
		private const string TmpDir = "temp";
		private const string VersionFile = "version";
		public static string Msvcr100DllHearthstonePath => Path.Combine(Config.Instance.HearthstoneDirectory, Msvcr100Dll);
		public static string Msvcr100DllPath => Path.Combine(HsReplayPath, Msvcr100Dll);
		public static string TmpDirPath => Path.Combine(HsReplayPath, TmpDir);
		public static string HsReplayPath => Path.Combine(Config.AppDataPath, HsReplayDir);
		public static string HsReplayExe => Path.Combine(HsReplayPath, HsReplayExeFilename);
		public static string VersionFilePath => Path.Combine(HsReplayPath, VersionFile);
	}
}