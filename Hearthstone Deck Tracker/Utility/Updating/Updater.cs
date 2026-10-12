using System;

namespace Hearthstone_Deck_Tracker.Utility.Updating
{
	internal static partial class Updater
	{
#if(SQUIRREL)
		public const string Name = "squirrel";
#elif(VELOPACK)
		public const string Name = "velopack";
#else
		public const string Name = "none";
#endif

		private static DateTime _lastUpdateCheck = DateTime.Now;
		public static UpdaterStatus Status { get; } = new();
	}
}
