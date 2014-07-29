using System;
using System.Diagnostics;

namespace Hearthstone_Deck_Tracker
{
	static class Logger
	{
		public static int LogLevel { private get; set; }


		/// <summary>
		/// Writes line to trace
		/// </summary>
		public static void WriteLine(string line, int logLevel = 0)
		{
			WriteLine(line, "", logLevel);
		}

		/// <summary>
		/// Writes line to trace
		/// </summary>
		public static void WriteLine(string line, string category, int logLevel = 0)
		{
			if (logLevel <= LogLevel)
				Trace.WriteLine(string.Format("[{0}]{1}: {2}", DateTime.Now.ToLongTimeString(), category, line));
		}
	}
}
