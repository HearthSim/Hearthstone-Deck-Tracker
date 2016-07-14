#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Handlers;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.API.LogEvents;
using static Hearthstone_Deck_Tracker.LogReader.HsLogReaderConstants;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class LogReaderManager
	{
		internal const int UpdateDelay = 100;
		private static readonly SortedList<DateTime, List<LogLineItem>> ToProcess = new SortedList<DateTime, List<LogLineItem>>();
		private static readonly List<LogReader> LogReaders = new List<LogReader>();
		private static readonly PowerHandler PowerLineHandler = new PowerHandler();
		private static readonly RachelleHandler RachelleHandler = new RachelleHandler();
		private static readonly ArenaHandler ArenaHandler = new ArenaHandler();
		private static readonly NetHandler NetHandler = new NetHandler();
		private static readonly LoadingScreenHandler LoadingScreenHandler = new LoadingScreenHandler();
		private static readonly FullScreenFxHandler FullScreenFxHandler = new FullScreenFxHandler();
		private static LogReader _gameStatePowerLogReader;
		private static LogReader _powerLogReader;
		private static LogReader _loadingScreenLogReader;
		private static HsGameState _gameState;
		private static GameV2 _game;
		private static DateTime _startingPoint;
		private static bool _stop;
		private static bool _running;

		private static void InitializeLogReaders()
		{
			_gameStatePowerLogReader = new LogReader(GameStatePowerLogReaderInfo);
			_powerLogReader = new LogReader(PowerLogReaderInfo);
			_loadingScreenLogReader = new LogReader(LoadingScreenLogReaderInfo);
			LogReaders.Add(_powerLogReader);
			LogReaders.Add(_loadingScreenLogReader);
			LogReaders.Add(new LogReader(NetLogReaderInfo));
			LogReaders.Add(new LogReader(RachelleLogReaderInfo));
			LogReaders.Add(new LogReader(ArenaLogReaderInfo));
			LogReaders.Add(new LogReader(FullScreenFxLogReaderInfo));
		}

		public static async Task Start(GameV2 game)
		{
			if(!Helper.HearthstoneDirExists)
				await FindHearthstone();
			InitializeGameState(game);
			InitializeLogReaders();
			_startingPoint = GetStartingPoint();
			StartLogReaders();
		}

		private static async Task FindHearthstone()
		{
			Log.Warn("Hearthstone not found, waiting for process...");
			Process proc;
			while((proc = User32.GetHearthstoneProc()) == null)
				await Task.Delay(500);
			var dir = new FileInfo(proc.MainModule.FileName).Directory?.FullName;
			if(dir == null)
			{
				const string msg = "Could not find Hearthstone installation";
				Log.Error(msg);
				ErrorManager.AddError(msg, "Please point HDT to your Hearthstone installation via 'options > tracker > settings > set hearthstone path'.");
				return;
			}
			Log.Info($"Found Hearthstone at '{dir}'");
			Config.Instance.HearthstoneDirectory = dir;
			Config.Save();
		}

		private static async void StartLogReaders()
		{
			if(_running)
				return;
			foreach(var logReader in LogReaders)
				logReader.Start(_startingPoint);
			_gameStatePowerLogReader.Start(_startingPoint);
			_running = true;
			_stop = false;
			var powerLines = new List<LogLineItem>();
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
					powerLines = _gameStatePowerLogReader.Collect().ToList();
				});
				ProcessNewLines();
				if(powerLines.Any())
				{
					Core.Game.PowerLog.AddRange(powerLines.Select(x => x.Line));
					powerLines.Clear();
				}
				await Task.Delay(UpdateDelay);
			}
			_running = false;
		}

		private static DateTime GetStartingPoint()
		{
			var powerEntry =
				_powerLogReader.FindEntryPoint(new[] {"tag=GOLD_REWARD_STATE", "End Spectator"});
			var lsEntry = _loadingScreenLogReader.FindEntryPoint("Gameplay.Start");
			return lsEntry > powerEntry ? lsEntry : powerEntry;
		}

		public static int GetTurnNumber() => _gameState.GetTurnNumber();

		public static async Task<bool> Stop(bool force = false)
		{
			if(!_running)
			{
				Log.Warn("LogReaders could not be stopped, stop already in progress.");
				return false;
			}
			_stop = true;
			while(_running)
				await Task.Delay(50);
			await Task.WhenAll(LogReaders.Where(x => force || x.Info.Reset).Concat(new[] {_gameStatePowerLogReader}).Select(x => x.Stop()));
			Log.Info("Stopped LogReaders.");
			PowerLineHandler.Reset();
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
			Log.Info("Restarting LogReaders.");
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
			_gameState.Reset();
		}

		private static void ProcessNewLines()
		{
			foreach(var item in ToProcess.Where(item => item.Value != null))
			{
				if(_stop)
					break;
				foreach(var line in item.Value.Where(line => line != null))
				{
					if(_stop)
						break;
					_game.GameTime.Time = line.Time;
					switch(line.Namespace)
					{
						case "Power":
							PowerLineHandler.Handle(line.Line, _gameState, _game);
							OnPowerLogLine.Execute(line.Line);
							break;
						case "Rachelle":
							RachelleHandler.Handle(line.Line, _gameState, _game);
							OnRachelleLogLine.Execute(line.Line);
							break;
						case "Arena":
							ArenaHandler.Handle(line, _gameState, _game);
							OnArenaLogLine.Execute(line.Line);
							break;
						case "LoadingScreen":
							LoadingScreenHandler.Handle(line, _gameState, _game);
							break;
						case "Net":
							NetHandler.Handle(line, _gameState, _game);
							break;
						case "FullScreenFX":
							FullScreenFxHandler.Handle(line, _game);
							break;
					}
				}
			}
			ToProcess.Clear();
			Helper.UpdateEverything(_game);
		}
	}
}