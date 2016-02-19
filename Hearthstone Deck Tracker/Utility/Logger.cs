#region

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker
{
	[DebuggerStepThrough]
	[Obsolete("Use Utiliy.Logging.Log")]
	public static class Logger
	{
		
		[Obsolete("Use Utiliy.Logging.Log")]
		public static void WriteLine(string line, int logLevel = 0)
		{
			WriteLine(line, "", logLevel);
		}

		[Obsolete("Use Utiliy.Logging.Log")]
		public static void WriteLine(string line, string category, int logLevel = 0)
		{
			Log.WriteLine(line, logLevel > 0 ? LogType.Debug : LogType.Info, category);
		}
	}
}