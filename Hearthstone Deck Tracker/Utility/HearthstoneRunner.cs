using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility
{
	internal class HearthstoneRunner
	{
		private static bool _starting;
		public static Action<bool> StartingHearthstone;

		public static async Task StartHearthstone()
		{
			if(_starting || User32.GetHearthstoneWindow() != IntPtr.Zero)
				return;
			_starting = true;
			StartingHearthstone?.Invoke(true);
			try
			{
				var bnetProc = GetProcess("Battle.net") ?? GetProcess("Battle.net.beta");
				if(bnetProc == null)
				{
					Process.Start("battlenet://");
					var foundBnetWindow = false;
					for(var i = 0; i < 40; i++)
					{
						bnetProc = GetProcess("Battle.net") ?? GetProcess("Battle.net.beta");
						if(bnetProc != null && bnetProc.MainWindowHandle != IntPtr.Zero)
						{
							foundBnetWindow = true;
							break;
						}

						await Task.Delay(500);
					}
					if(foundBnetWindow)
					{
						ErrorManager.AddError("Could not start Battle.net Launcher",
							"Starting the Battle.net launcher failed or was too slow. "
							+ "Please try again once it started or run Hearthstone manually.", true);
						return;
					}
				}

				await Task.Delay(2000);
				Process.Start("battlenet://WTCG");
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			finally
			{
				_starting = false;
				await Task.Delay(2000);
				StartingHearthstone?.Invoke(false);
			}
		}

		private static Process GetProcess(string name)
		{
			return Process.GetProcessesByName(name).FirstOrDefault();
		}
	}
}
