using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility
{
	public readonly struct TimedSection : IDisposable
	{
		private readonly string _label;
		private readonly Stopwatch _stopwatch;
		private readonly string _memberName;
		private readonly string _sourceFilePath;

		public TimedSection(string label, [CallerMemberName] string memberName = "",
							 [CallerFilePath] string sourceFilePath = "")
		{
			_label = label;
			_memberName = memberName;
			_sourceFilePath = sourceFilePath;
			_stopwatch = Stopwatch.StartNew();
		}

		public void Dispose()
		{
			_stopwatch.Stop();
			Log.Info($"{_label} took {_stopwatch.ElapsedMilliseconds}ms", _memberName, _sourceFilePath);
		}
	}
}
