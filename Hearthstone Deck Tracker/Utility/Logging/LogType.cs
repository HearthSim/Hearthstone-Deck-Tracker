#region

using System;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.Logging
{
	[Flags]
	public enum LogType
	{
		Debug,
		Info,
		Warning,
		Error
	}
}
