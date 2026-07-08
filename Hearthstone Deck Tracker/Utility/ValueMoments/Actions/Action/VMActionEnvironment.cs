using System.Runtime.InteropServices;
using Hearthstone_Deck_Tracker.HsReplay;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public interface IVMActionEnvironment
	{
		bool IsAuthenticated { get; }
		int ScreenHeight { get; }
		int ScreenWidth { get; }
		string OsArch { get; }
		string OsVersion { get; }
	}

	internal class VMActionEnvironment : IVMActionEnvironment
	{
		public bool IsAuthenticated => HSReplayNetOAuth.IsFullyAuthenticated;
		public int ScreenHeight => Helper.GetHearthstoneMonitorRect().Height;
		public int ScreenWidth => Helper.GetHearthstoneMonitorRect().Width;
		public string OsArch => RuntimeInformation.OSArchitecture.ToString().ToLower();
		public string OsVersion => Helper.GetWindowsMajorVersionName();
	}
}
