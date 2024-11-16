using System;

namespace Hearthstone_Deck_Tracker.Utility.Updating
{
	internal static partial class Updater
	{
		private static DateTime _lastUpdateCheck = DateTime.Now;
		public static UpdaterStatus Status { get; } = new();
	}
}
