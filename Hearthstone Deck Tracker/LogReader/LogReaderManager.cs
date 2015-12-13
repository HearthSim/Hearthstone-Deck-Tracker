﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Handlers;

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
		private static readonly LoadingScreenHandler LoadingScreenHandler = new LoadingScreenHandler();
		private static LogReader _powerLogReader;
		private static LogReader _bobLogReader;
		private static HsGameState _gameState;
		private static GameV2 _game;
		private static DateTime _startingPoint;
		private static bool _stop;
		private static bool _running;

		private static void InitializeLogReaders()
		{
			_powerLogReader = new LogReader(HsLogReaderConstants.PowerLogReaderInfo);
			_bobLogReader = new LogReader(HsLogReaderConstants.BobLogReaderInfo);
			LogReaders.Add(_powerLogReader);
			LogReaders.Add(_bobLogReader);
			LogReaders.Add(new LogReader(HsLogReaderConstants.RachelleLogReaderInfo));
			LogReaders.Add(new LogReader(HsLogReaderConstants.AssetLogReaderInfo));
			LogReaders.Add(new LogReader(HsLogReaderConstants.ArenaLogReaderInfo));
			LogReaders.Add(new LogReader(HsLogReaderConstants.LoadingScreenLogReaderInfo));
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
			var powerEntry = _powerLogReader.FindEntryPoint(new [] {"GameState.DebugPrintPower() - CREATE_GAME", "tag=GOLD_REWARD_STATE", "End Spectator" });
			var bobEntry = _bobLogReader.FindEntryPoint("legend rank");
			return powerEntry > bobEntry ? powerEntry : bobEntry;
		}

		public static int GetTurnNumber()
		{
			return _gameState.GetTurnNumber();
		}

		public static async Task<bool> Stop()
		{
			if(!_running)
			{
				Logger.WriteLine("LogReaders could not be stopped, stop already in progress.", "LogReaderManager");
				return false;
			}
			_stop = true;
			while(_running)
				await Task.Delay(50);
			await Task.WhenAll(LogReaders.Select(x => x.Stop()));
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
							API.LogEvents.OnPowerLogLine.Execute(line.Line);
							break;
						case "Asset":
							AssetHandler.Handle(line.Line, _gameState, _game);
							API.LogEvents.OnAssetLogLine.Execute(line.Line);
							break;
						case "Bob":
							BobHandler.Handle(line.Line, _gameState, _game);
                            API.LogEvents.OnBobLogLine.Execute(line.Line);
							break;
						case "Rachelle":
							RachelleHandler.Handle(line.Line, _gameState, _game);
							API.LogEvents.OnRachelleLogLine.Execute(line.Line);
							break;
						case "Arena":
							ArenaHandler.Handle(line.Line, _gameState, _game);
							API.LogEvents.OnArenaLogLine.Execute(line.Line);
							break;
						case "LoadingScreen":
							LoadingScreenHandler.Handle(line.Line, _gameState, _game);
							break;
					}
				}
			}
			ToProcess.Clear();
			Helper.UpdateEverything(_game);
		}
	}
}