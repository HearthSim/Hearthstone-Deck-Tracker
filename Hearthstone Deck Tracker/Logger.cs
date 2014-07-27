using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker
{
	static class Logger
	{
		public static int _logLevel { private get; set; }


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
			if (logLevel <= _logLevel)
				Trace.WriteLine(string.Format("[{0}]{1}: {2}", DateTime.Now.ToLongTimeString(), category, line));
		}
	}
}
