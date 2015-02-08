#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker.Stats
{
	public class DefaultDeckStats
	{
		private static DefaultDeckStats _instance;
		public List<DeckStats> DeckStats;

		public DefaultDeckStats()
		{
			DeckStats = new List<DeckStats>();
		}

		public static DefaultDeckStats Instance
		{
			get { return _instance ?? (_instance = new DefaultDeckStats()); }
		}

		public DeckStats GetDeckStats(string hero)
		{
			if(!Enum.GetNames(typeof(HeroClass)).Contains(hero))
				return null;
			var ds = DeckStats.FirstOrDefault(d => d.Name == hero);
			if(ds == null)
			{
				ds = new DeckStats {Name = hero};
				DeckStats.Add(ds);
			}
			return ds;
		}

		public static void Load()
		{
			var file = Config.Instance.DataDir + "DefaultDeckStats.xml";
			if(!File.Exists(file))
				return;
			try
			{
				_instance = XmlManager<DefaultDeckStats>.Load(file);
			}
			catch(Exception ex)
			{
				//failed loading deckstats 
				var corruptedFile = Helper.GetValidFilePath(Config.Instance.DataDir, "DefaultDeckStats_corrupted", "xml");
				try
				{
					File.Move(file, corruptedFile);
				}
				catch(Exception)
				{
					throw new Exception(
						"Can not load or move DefaultDeckStats.xml file. Please manually delete the file in \"%appdata\\HearthstoneDeckTracker\".");
				}

				//get latest backup file
				var backup =
					new DirectoryInfo(Config.Instance.DataDir).GetFiles("DefaultDeckStats_backup*")
					                                          .OrderByDescending(x => x.CreationTime)
					                                          .FirstOrDefault();
				if(backup != null)
				{
					try
					{
						File.Copy(backup.FullName, file);
						_instance = XmlManager<DefaultDeckStats>.Load(file);
					}
					catch(Exception ex2)
					{
						throw new Exception(
							"Error restoring DefaultDeckStats backup. Please manually rename \"DefaultDeckStats_backup.xml\" to \"DefaultDeckStats.xml\" in \"%appdata\\HearthstoneDeckTracker\".",
							ex2);
					}
				}
				else
					throw new Exception("DefaultDeckStats.xml is corrupted.", ex);
			}
		}

		public static void Save()
		{
			var file = Config.Instance.DataDir + "DefaultDeckStats.xml";
			XmlManager<DefaultDeckStats>.Save(file, Instance);
		}
	}
}