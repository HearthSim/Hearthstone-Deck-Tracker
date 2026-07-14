using System.Runtime.InteropServices;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public interface IVMActionEnvironment
	{
		bool IsAuthenticated { get; }
		int ScreenHeight { get; }
		int ScreenWidth { get; }
		string OsArch { get; }
		string OsMajor { get; }
		string OsVersion { get; }
		string AppVersion { get; }
	}

	internal class VMActionEnvironment : IVMActionEnvironment
	{
		public bool IsAuthenticated => HSReplayNetOAuth.IsFullyAuthenticated;
		public int ScreenHeight => Helper.GetHearthstoneMonitorRect().Height;
		public int ScreenWidth => Helper.GetHearthstoneMonitorRect().Width;
		public string OsArch => RuntimeInformation.OSArchitecture.ToString().ToLower();
		public string OsMajor => Helper.GetWindowsMajorVersionName();
		public string OsVersion => Helper.GetFullWindowsVersion();
		public string AppVersion => Helper.GetCurrentVersion().ToVersionString();
	}
}
