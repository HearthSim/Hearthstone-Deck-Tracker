#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Stats
{
	public class DefaultDeckStats
	{
		private static Lazy<DefaultDeckStats> _instance = new Lazy<DefaultDeckStats>(Load);
		public List<DeckStats> DeckStats;

		private DefaultDeckStats()
		{
			DeckStats = new List<DeckStats>();
		}

		static DefaultDeckStats()
		{
		}

		public static DefaultDeckStats Instance => _instance.Value;

		public DeckStats GetDeckStats(string hero)
		{
			if(string.IsNullOrEmpty(hero))
				return null;
			var ds = DeckStats.FirstOrDefault(d => d.Name == hero);
			if(ds != null)
				return ds;
			ds = new DeckStats {Name = hero};
			DeckStats.Add(ds);
			return ds;
		}

		private static DefaultDeckStats Load()
		{
			SetupDefaultDeckStatsFile();
			var file = Config.Instance.DataDir + "DefaultDeckStats.xml";
			if(!File.Exists(file))
				return new DefaultDeckStats();
			try
			{
				return XmlManager<DefaultDeckStats>.Load(file);
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
						return XmlManager<DefaultDeckStats>.Load(file);
					}
					catch(Exception ex2)
					{
						throw new Exception(
							"Error restoring DefaultDeckStats backup. Please manually rename \"DefaultDeckStats_backup.xml\" to \"DefaultDeckStats.xml\" in \"%appdata\\HearthstoneDeckTracker\".",
							ex2);
					}
				}
				throw new Exception("DefaultDeckStats.xml is corrupted.", ex);
			}
		}


		internal static void SetupDefaultDeckStatsFile()
		{
			if(Config.Instance.SaveDataInAppData == null)
				return;
			var appDataPath = Config.AppDataPath + @"\DefaultDeckStats.xml";
			var dataDirPath = Config.Instance.DataDirPath + @"\DefaultDeckStats.xml";
			if(Config.Instance.SaveDataInAppData.Value)
			{
				if(File.Exists(dataDirPath))
				{
					if(File.Exists(appDataPath))
					{
						//backup in case the file already exists
						var time = DateTime.Now.ToFileTime();
						File.Move(appDataPath, appDataPath + time);
						Log.Info("Created backups of DefaultDeckStats in appdata");
					}
					File.Move(dataDirPath, appDataPath);
					Log.Info("Moved DefaultDeckStats to appdata");
				}
			}
			else if(File.Exists(appDataPath))
			{
				if(File.Exists(dataDirPath))
				{
					//backup in case the file already exists
					var time = DateTime.Now.ToFileTime();
					File.Move(dataDirPath, dataDirPath + time);
					Log.Info("Created backups of DefaultDeckStats locally");
				}
				File.Move(appDataPath, dataDirPath);
				Log.Info("Moved DefaultDeckStats to local");
			}

			var filePath = Config.Instance.DataDir + "DefaultDeckStats.xml";
			//create if it does not exist
			if(!File.Exists(filePath))
			{
				using(var sr = new StreamWriter(filePath, false))
					sr.WriteLine("<DefaultDeckStats></DefaultDeckStats>");
			}
		}


		public static void Save() => XmlManager<DefaultDeckStats>.Save(Config.Instance.DataDir + "DefaultDeckStats.xml", Instance);

		internal static void Reload() => _instance = new Lazy<DefaultDeckStats>(Load);
	}
}