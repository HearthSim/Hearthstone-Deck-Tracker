#region

using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Protocol;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	internal class PipeServer
	{
		internal static void StartAll()
		{
			StartGeneralServer();
			StartImportServer();
		}

		private static async void StartImportServer()
		{
			Log.Info("Started server");
			var exceptionCount = 0;
			while(exceptionCount < 10)
			{
				try
				{
					using(var pipe = new NamedPipeServerStream("hdtimport", PipeDirection.In, 1, PipeTransmissionMode.Message))
					{
						Log.Info("Waiting for connecetion...");
						await Task.Run(() => pipe.WaitForConnection());
						using(var sr = new StreamReader(pipe))
						{
							var line = sr.ReadLine();
							var decks = JsonConvert.DeserializeObject<JsonDecksWrapper>(line);
							decks.SaveDecks();
							Log.Info(line);
						}
					}
				}
				catch(Exception ex)
				{
					Log.Error(ex);
					exceptionCount++;
				}
			}
			Log.Info("Closed server. ExceptionCount=" + exceptionCount);
		}

		private static async void StartGeneralServer()
		{
			Log.Info("Started server");
			var exceptionCount = 0;
			while(exceptionCount < 10)
			{
				try
				{
					using(var pipe = new NamedPipeServerStream("hdtgeneral", PipeDirection.In, 1, PipeTransmissionMode.Message))
					{
						Log.Info("Waiting for connecetion...");
						await Task.Run(() => pipe.WaitForConnection());
						using(var sr = new StreamReader(pipe))
						{
							var line = sr.ReadLine();
							switch(line)
							{
								case "sync":
									HearthStatsManager.SyncAsync(false, true);
									break;
							}
							Log.Info(line);
						}
					}
				}
				catch(Exception ex)
				{
					Log.Error(ex);
					exceptionCount++;
				}
			}
			Log.Info("Closed server. ExceptionCount=" + exceptionCount);
		}
	}
}