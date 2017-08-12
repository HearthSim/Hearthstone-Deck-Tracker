#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class DataIssueResolver
	{
		internal static bool RunDeckStatsFix;
		public static void Run()
		{
			if(RunDeckStatsFix)
				FixDeckStats();
			if(Directory.Exists("Images/Bars"))
				CleanUpImageFiles();
		}

		private static void CleanUpImageFiles()
		{
			Log.Info("Cleaning up old card image files...");
			try
			{
				Directory.Delete("Images/Bars", true);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			Log.Info("Cleanup complete.");
		}

		//https://github.com/HearthSim/Hearthstone-Deck-Tracker/issues/2675
		private static void FixDeckStats()
		{
			var save = false;
			foreach(var d in DeckList.Instance.Decks.Where(d => d.DeckStats.DeckId != d.DeckId))
			{
				if(!DeckStatsList.Instance.DeckStats.TryGetValue(d.DeckId, out var deckStats))
					continue;
				foreach(var game in deckStats.Games.ToList())
				{
					deckStats.Games.Remove(game);
					d.DeckStats.Games.Add(game);
				}
				save = true;
			}
			if(save)
			{
				DeckStatsList.Save();
				Core.MainWindow.DeckPickerList.UpdateDecks();
			}
		}
	}
}
