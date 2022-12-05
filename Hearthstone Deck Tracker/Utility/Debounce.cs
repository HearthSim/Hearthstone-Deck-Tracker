using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class Debounce
	{
		private static readonly Dictionary<string, int> _debounces = new();

		public static async Task<bool> WasCalledAgain( int milliseconds, [CallerMemberName]string callerMemberName = "", [CallerFilePath]string callerFilePath = "" )
		{
			var id = $"{callerMemberName}.{callerFilePath}";
			if(!_debounces.ContainsKey(id))
				_debounces[id] = 0;
			var count = ++_debounces[id];
			await Task.Delay(milliseconds);
			return _debounces[id] != count;
		}
	}
}
