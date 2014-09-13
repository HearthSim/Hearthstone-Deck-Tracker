using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker.Stats
{
	public class DeckStats
	{
		[XmlArray(ElementName = "Games")]
		[XmlArrayItem(ElementName = "Game")]
		public List<GameStats> Games;

		public string Name;

		public DeckStats()
		{
			Games = new List<GameStats>();
		}

		public DeckStats(string name)
		{
			Name = name;
			Games = new List<GameStats>();
		}

		public void AddGameResult(GameResult result, string opponentHero)
		{
			Games.Add(new GameStats(result, opponentHero));
		}

		public void AddGameResult(GameStats gameStats)
		{
			Games.Add(gameStats);
		}
	}
			try
			{
				_instance = XmlManager<DeckStatsList>.Load(file);
			}
			catch(Exception)
			{
				//failed loading deckstats 
				var corruptedFile = Helper.GetValidFilePath(Config.Instance.DataDir, "DeckStats_corrupted", "xml");
				try
				{
					File.Move(file, corruptedFile);
				}
				catch(Exception)
				{
					throw new Exception("Can not load or move DeckStats.xml file. Please manually delete the file in \"%appdata\\HearthstoneDeckTracker\".");
				}

				//get latest backup file
				var backup =
					new DirectoryInfo(Config.Instance.DataDir).GetFiles("DeckStats_backup*")
					                                          .OrderByDescending(x => x.CreationTime)
					                                          .FirstOrDefault();
				if(backup != null)
				{
					try
					{
						File.Copy(backup.FullName, file);
						_instance = XmlManager<DeckStatsList>.Load(file);
					}
					catch(Exception)
					{
						throw new Exception("Error restoring DeckStats backup. Please manually rename \"DeckStats_backup.xml\" to \"DeckStats.xml\" in \"%appdata\\HearthstoneDeckTracker\".");
					}
				}
				else
				{
					//can't call ShowMessageAsync on MainWindow at this point. todo: Add something like a message queue.
					MessageBox.Show("Your DeckStats file got corrupted and there was no backup to restore from.", "Error restoring DeckStats backup");
				}
			}
}