#region

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Handlers;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using HearthWatcher;
using HearthWatcher.LogReader;
using static Hearthstone_Deck_Tracker.API.LogEvents;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class LogWatcherManager
	{
		private readonly PowerHandler _powerLineHandler = new PowerHandler();
		private readonly GameplayHandler _rachelleHandler = new GameplayHandler();
		private readonly ArenaHandler _arenaHandler = new ArenaHandler();
		private readonly LoadingScreenHandler _loadingScreenHandler = new LoadingScreenHandler();
		private readonly FullScreenFxHandler _fullScreenFxHandler = new FullScreenFxHandler();
		private HsGameState _gameState;
		private GameV2 _game;
		private readonly LogWatcher _logWatcher;
		private bool _stop;

		public static LogWatcherInfo AchievementsLogWatcherInfo => new LogWatcherInfo { Name = "Achievements" };
		public static LogWatcherInfo PowerLogWatcherInfo => new LogWatcherInfo
		{
			Name = "Power",
			StartsWithFilters = new[] {"PowerTaskList.DebugPrintPower", "GameState."},
			ContainsFilters = new[] {"Begin Spectating", "Start Spectator", "End Spectator"}
		};

		public static LogWatcherInfo GameplayLogWatcherInfo => new LogWatcherInfo {Name = "Gameplay"};
		public static LogWatcherInfo ArenaLogWatcherInfo => new LogWatcherInfo {Name = "Arena", Reset = false};
		public static LogWatcherInfo LoadingScreenLogWatcherInfo => new LogWatcherInfo {Name = "LoadingScreen", StartsWithFilters = new[] {"LoadingScreen.OnSceneLoaded", "Gameplay" } };
		public static LogWatcherInfo FullScreenFxLogWatcherInfo => new LogWatcherInfo { Name = "FullScreenFX", Reset = false};

		public LogWatcherManager()
		{
			_logWatcher = new LogWatcher(new []
			{
				AchievementsLogWatcherInfo,
				PowerLogWatcherInfo,
				GameplayLogWatcherInfo,
				ArenaLogWatcherInfo,
				LoadingScreenLogWatcherInfo,
				FullScreenFxLogWatcherInfo
			});
			_logWatcher.OnNewLines += OnNewLines;
			_logWatcher.OnLogFileFound += OnLogFileFound;
			_logWatcher.OnLogLineIgnored += OnLogLineIgnored;

			_loadingScreenHandler.OnHearthMirrorCheckFailed += OnHearthMirroCheckFailed;
		}

		private void OnLogFileFound(string msg) => Log.Info(msg);
		private void OnLogLineIgnored(string msg) => Log.Warn(msg);

		private async void OnHearthMirroCheckFailed()
		{
			await Stop(true);
			Core.MainWindow.ActivateWindow();
			while(Core.MainWindow.Visibility != Visibility.Visible || Core.MainWindow.WindowState == WindowState.Minimized)
				await Task.Delay(100);
			await Core.MainWindow.ShowMessage("Uneven permissions",
				"It appears that Hearthstone (Battle.net) and HDT do not have the same permissions.\n\nPlease run both as administrator or local user.\n\nIf you don't know what any of this means, just run HDT as administrator.");
		}

		public async Task Start(GameV2 game)
		{
			if(!Helper.HearthstoneDirExists)
				await FindHearthstone();
			InitializeGameState(game);
			_stop = false;
			var logDirectory = Path.Combine(Config.Instance.HearthstoneDirectory, Config.Instance.HearthstoneLogsDirectoryName);
			Log.Info($"Using Hearthstone log directory '{logDirectory}'");
			_logWatcher.Start(logDirectory);
		}

		private async Task FindHearthstone()
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

		public async Task<bool> Stop(bool force = false)
		{
			_stop = true;
			return await _logWatcher.Stop(force);
		}

		private void InitializeGameState(GameV2 game)
		{
			_game = game;
			_gameState = new HsGameState(game) { GameHandler = new GameEventHandler(game) };
			_gameState.Reset();
		}

		private void OnNewLines(List<LogLine> lines)
		{
			foreach(var line in lines)
			{
				if(_stop)
					break;
				_game.GameTime.Time = line.Time;
				switch(line.Namespace)
				{
					case "Achievements":
						OnAchievementsLogLine.Execute(line.Line);
						break;
					case "Power":
						if(line.LineContent.StartsWith("GameState."))
							_game.PowerLog.Add(line.Line);
						else
						{
							_powerLineHandler.Handle(line.Line, _gameState, _game);
							OnPowerLogLine.Execute(line.Line);
						}
						break;
					case "Gameplay":
						_rachelleHandler.Handle(line.Line, _gameState, _game);
						OnGameplayLogLine.Execute(line.Line);
						break;
					case "Arena":
						_arenaHandler.Handle(line, _gameState, _game);
						OnArenaLogLine.Execute(line.Line);
						break;
					case "LoadingScreen":
						_loadingScreenHandler.Handle(line, _gameState, _game);
						break;
					case "FullScreenFX":
						_fullScreenFxHandler.Handle(line, _game);
						break;
				}
			}
			Helper.UpdateEverything(_game);
		}
	}
}
