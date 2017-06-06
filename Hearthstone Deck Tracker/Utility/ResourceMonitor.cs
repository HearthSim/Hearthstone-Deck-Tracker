using System.Diagnostics;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class ResourceMonitor
	{
		public static async void Run()
		{
			while(true)
			{
				var mem = Process.GetCurrentProcess().PrivateMemorySize64 >> 20;
				if(mem > 1024)
				{ 
					Log.Warn($"High memory usage: {mem} MB");
					Influx.OnHighMemoryUsage(mem);
					return;
				}
				await Task.Delay(60000);
			}
		}
	}
}
