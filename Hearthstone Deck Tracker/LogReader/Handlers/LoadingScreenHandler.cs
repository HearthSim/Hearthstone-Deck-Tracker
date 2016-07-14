#region

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HearthMirror.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class LoadingScreenHandler
	{
		private DateTime _lastAutoImport;
		private bool _checkedMirrorStatus;

		public void Handle(LogLineItem logLine, IHsGameState gameState, IGame game)
		{
			var match = HsLogReaderConstants.GameModeRegex.Match(logLine.Line);
			if(match.Success)
			{
				game.CurrentMode = GetMode(match.Groups["curr"].Value);
				game.PreviousMode = GetMode(match.Groups["prev"].Value);

				if((DateTime.Now - logLine.Time).TotalSeconds < 5 && _lastAutoImport < logLine.Time
					&& game.CurrentMode == Mode.TOURNAMENT)
				{
					_lastAutoImport = logLine.Time;
					var decks = DeckImporter.FromConstructed();
					if(decks.Any() && (Config.Instance.ConstructedAutoImportNew || Config.Instance.ConstructedAutoUpdate))
						DeckManager.ImportDecks(decks, false, Config.Instance.ConstructedAutoImportNew,
							Config.Instance.ConstructedAutoUpdate);
				}

				if(game.PreviousMode == Mode.GAMEPLAY)
					gameState.GameHandler.HandleInMenu();

				if(game.CurrentMode == Mode.HUB && !_checkedMirrorStatus && (DateTime.Now - logLine.Time).TotalSeconds < 5)
					CheckMirrorStatus();
			}
			else if(logLine.Line.Contains("Gameplay.Start"))
			{
				gameState.Reset();
				gameState.GameHandler.HandleGameStart();
			}
		}

		private async void CheckMirrorStatus()
		{
			_checkedMirrorStatus = true;
			HearthMirror.Status status;
			while((status = HearthMirror.Status.GetStatus()).MirrorStatus == MirrorStatus.ProcNotFound)
				await Task.Delay(1000);
			Log.Info($"Mirror status: {status.MirrorStatus}");
			if(status.MirrorStatus != MirrorStatus.Error)
				return;
			Log.Error(status.Exception);
			if(!(status.Exception is Win32Exception))
			{
				Log.Info("Not a Win32Exception - Process probably exited. Checking again later.");
				_checkedMirrorStatus = false;
				return;
			}
			LogReaderManager.Stop(true).Forget();
			Core.MainWindow.ActivateWindow();
			while(Core.MainWindow.Visibility != Visibility.Visible || Core.MainWindow.WindowState == WindowState.Minimized)
				await Task.Delay(100);
			await Core.MainWindow.ShowMessage("Uneven permissions",
				"It appears that Hearthstone (Battle.net) and HDT do not have the same permissions.\n\nPlease run both as administrator or local user.\n\nIf you don't know what any of this means, just run HDT as administrator.");
		}

		private Mode GetMode(string modeString)
		{
			Mode mode;
			if(Enum.TryParse(modeString, out mode))
				return mode;
			return Mode.INVALID;
		}
	}
}