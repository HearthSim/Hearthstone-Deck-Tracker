#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Handlers;
using static Hearthstone_Deck_Tracker.API.LogEvents;
using static Hearthstone_Deck_Tracker.LogReader.HsLogReaderConstants;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class LogReaderManager
	{
		private static readonly SortedList<DateTime, List<LogLineItem>> ToProcess = new SortedList<DateTime, List<LogLineItem>>();
		private static readonly List<LogReader> LogReaders = new List<LogReader>();
		private static readonly PowerHandler PowerLineHandler = new PowerHandler();
		private static readonly RachelleHandler RachelleHandler = new RachelleHandler();
		private static readonly AssetHandler AssetHandler = new AssetHandler();
		private static readonly BobHandler BobHandler = new BobHandler();
		private static readonly ArenaHandler ArenaHandler = new ArenaHandler();
		private static readonly NetHandler NetHandler = new NetHandler();
		private static readonly LoadingScreenHandler LoadingScreenHandler = new LoadingScreenHandler();
		private static LogReader _powerLogReader;
		private static LogReader _bobLogReader;
		private static LogReader _netLogReader;
		private static HsGameState _gameState;
		private static GameV2 _game;
		private static DateTime _startingPoint;
		private static bool _stop;
		private static bool _running;

		private static void InitializeLogReaders()
		{
			_powerLogReader = new LogReader(PowerLogReaderInfo);
			_bobLogReader = new LogReader(BobLogReaderInfo);
			_netLogReader = new LogReader(NetLogReaderInfo);
			LogReaders.Add(_powerLogReader);
			LogReaders.Add(_bobLogReader);
			LogReaders.Add(_netLogReader);
			LogReaders.Add(new LogReader(RachelleLogReaderInfo));
			LogReaders.Add(new LogReader(AssetLogReaderInfo));
			LogReaders.Add(new LogReader(ArenaLogReaderInfo));
			LogReaders.Add(new LogReader(LoadingScreenLogReaderInfo));
		}

		public static void Start(GameV2 game)
		{
			InitializeGameState(game);
			InitializeLogReaders();
			_startingPoint = GetStartingPoint();
			StartLogReaders();
		}

		private static async void StartLogReaders()
		{
			if(_running)
				return;
			foreach(var logReader in LogReaders)
				logReader.Start(_startingPoint);
			_running = true;
			_stop = false;
			while(!_stop)
			{
				await Task.Factory.StartNew(() =>
				{
					foreach(var logReader in LogReaders)
					{
						var lines = logReader.Collect();
						foreach(var line in lines)
						{
							List<LogLineItem> logLines;
							if(!ToProcess.TryGetValue(line.Time, out logLines))
								ToProcess.Add(line.Time, logLines = new List<LogLineItem>());
							logLines.Add(line);
						}
					}
				});
				ProcessNewLines();
				await Task.Delay(Config.Instance.UpdateDelay);
			}
			_running = false;
		}

		private static DateTime GetStartingPoint()
		{
			var powerEntry =
				_powerLogReader.FindEntryPoint(new[] {"tag=GOLD_REWARD_STATE", "End Spectator"});
			var bobEntry = _bobLogReader.FindEntryPoint("legend rank");
			var powerBob = powerEntry > bobEntry ? powerEntry : bobEntry;
			var netEntry = _netLogReader.FindEntryPoint("ConnectAPI.GotoGameServer");
			return netEntry > powerBob ? netEntry : powerBob;
		}

		public static int GetTurnNumber() => _gameState.GetTurnNumber();

		public static async Task<bool> Stop(bool force = false)
		{
			if(!_running)
			{
				Logger.WriteLine("LogReaders could not be stopped, stop already in progress.", "LogReaderManager");
				return false;
			}
			_stop = true;
			while(_running)
				await Task.Delay(50);
			await Task.WhenAll(LogReaders.Where(x => force || x.Info.Reset).Select(x => x.Stop()));
			Logger.WriteLine("Stopped LogReaders.", "LogReaderManager");
			return true;
		}

		/// <summary>
		/// LogReaderManager.Stop needs to be called first!
		/// These can not happen in one call because other things need to be reset between stopping and restarting.
		/// </summary>
		public static void Restart()
		{
			if(_running)
				return;
			Logger.WriteLine("Restarting LogReaders.", "LogReaderManager");
			_startingPoint = GetStartingPoint();
			_gameState.Reset();
			_game.GameTime.TimedTasks.Clear();
			StartLogReaders();
		}

		private static void InitializeGameState(GameV2 game)
		{
			_game = game;
			_gameState = new HsGameState(game);
			_gameState.GameHandler = new GameEventHandler(game);
			_gameState.GameHandler.ResetConstructedImporting();
			_gameState.Reset();
		}

		private static void ProcessNewLines()
		{
			foreach(var item in ToProcess.Where(item => item.Value != null))
			{
				foreach(var line in item.Value.Where(line => line != null))
				{
					_game.GameTime.Time = line.Time;
					switch(line.Namespace)
					{
						case "Power":
							GameV2.AddHSLogLine(line.Line);
							PowerLineHandler.Handle(line.Line, _gameState, _game);
							OnPowerLogLine.Execute(line.Line);
							break;
						case "Asset":
							AssetHandler.Handle(line.Line, _gameState, _game);
							OnAssetLogLine.Execute(line.Line);
							break;
						case "Bob":
							BobHandler.Handle(line.Line, _gameState, _game);
							OnBobLogLine.Execute(line.Line);
							break;
						case "Rachelle":
							RachelleHandler.Handle(line.Line, _gameState, _game);
							OnRachelleLogLine.Execute(line.Line);
							break;
						case "Arena":
							ArenaHandler.Handle(line.Line, _gameState, _game);
							OnArenaLogLine.Execute(line.Line);
							break;
						case "LoadingScreen":
							LoadingScreenHandler.Handle(line.Line, _gameState, _game);
							break;
						case "Net":
							NetHandler.Handle(line.Line, _game);
							break;
					}
				}
			}
			ToProcess.Clear();
			Helper.UpdateEverything(_game);
		}
	}
}