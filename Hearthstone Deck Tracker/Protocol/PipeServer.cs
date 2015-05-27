#region

using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Protocol;
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
			Logger.WriteLine("Started server", "ImportPipe");
			var exceptionCount = 0;
			while(exceptionCount < 10)
			{
				try
				{
					using(var pipe = new NamedPipeServerStream("hdtimport", PipeDirection.In, 1, PipeTransmissionMode.Message))
					{
						Logger.WriteLine("Waiting for connecetion...", "ImportPipe");
						await Task.Run(() => pipe.WaitForConnection());
						using(var sr = new StreamReader(pipe))
						{
							var line = sr.ReadLine();
							var decks = JsonConvert.DeserializeObject<JsonDecksWrapper>(line);
							decks.SaveDecks();
							Logger.WriteLine(line, "ImportPipe");
						}
					}
				}
				catch(Exception ex)
				{
					Logger.WriteLine(ex.ToString(), "ImportPipe");
					exceptionCount++;
				}
			}
			Logger.WriteLine("Closed server. ExceptionCount=" + exceptionCount, "ImportPipe");
		}

		private static async void StartGeneralServer()
		{
			Logger.WriteLine("Started server", "GeneralPipe");
			var exceptionCount = 0;
			while(exceptionCount < 10)
			{
				try
				{
					using(var pipe = new NamedPipeServerStream("hdtgeneral", PipeDirection.In, 1, PipeTransmissionMode.Message))
					{
						Logger.WriteLine("Waiting for connecetion...", "GeneralPipe");
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
							Logger.WriteLine(line, "ImportPipe");
						}
					}
				}
				catch(Exception ex)
				{
					Logger.WriteLine(ex.ToString(), "GeneralPipe");
					exceptionCount++;
				}
			}
			Logger.WriteLine("Closed server. ExceptionCount=" + exceptionCount, "GeneralPipe");
		}
	}
}