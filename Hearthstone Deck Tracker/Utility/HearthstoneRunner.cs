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
		private const int ProcCheckInterval = 500;
		private const int ProcExistDuration = 7000;
		private const int TryStartBnetDuration = 20000;
		private const int StartBnetTries = TryStartBnetDuration / ProcCheckInterval;
		private const int EventDelay = 2000;

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
					var foundDuration = 0;
					for(var i = 0; i < StartBnetTries; i++)
					{
						bnetProc = GetProcess("Battle.net") ?? GetProcess("Battle.net.beta");
						if(bnetProc != null && bnetProc.MainWindowHandle != IntPtr.Zero)
						{
							if((foundDuration += ProcCheckInterval) >= ProcExistDuration)
								break;
						}
						await Task.Delay(ProcCheckInterval);
					}
					if(foundDuration == 0)
					{
						ErrorManager.AddError("Could not start Battle.net Launcher",
							"Starting the Battle.net launcher failed or was too slow. "
							+ "Please try again once it started or run Hearthstone manually.", true);
						return;
					}
				}
				if(bnetProc != null)
					Process.Start(bnetProc.MainModule.FileName, "--exec=\"launch WTCG\"");
				else
					ErrorManager.AddError("Could not launch Hearthstone",
							"Please try again or launch Hearthstone manually.", true);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			finally
			{
				_starting = false;
				await Task.Delay(EventDelay);
				StartingHearthstone?.Invoke(false);
			}
		}

		private static Process GetProcess(string name)
		{
			return Process.GetProcessesByName(name).FirstOrDefault();
		}
	}
}
