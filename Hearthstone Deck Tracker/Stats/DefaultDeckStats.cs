#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility;
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
#if(!SQUIRREL)
			SetupDefaultDeckStatsFile();
#endif
			var file = Path.Combine(Config.Instance.DataDir, "DefaultDeckStats.xml");
			if(!File.Exists(file))
				return new DefaultDeckStats();
			try
			{
				return XmlManager<DefaultDeckStats>.Load(file);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				try
				{
					File.Move(file, Helper.GetValidFilePath(Config.Instance.DataDir, "DefaultDeckStats_corrupted", "xml"));
				}
				catch(Exception ex1)
				{
					Log.Error(ex1);
				}
				return BackupManager.TryRestore<DefaultDeckStats>("DefaultDeckStats.xml") ?? new DefaultDeckStats();
			}
		}

#if(!SQUIRREL)
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
		}
#endif

		public static void Save() => XmlManager<DefaultDeckStats>.Save(Config.Instance.DataDir + "DefaultDeckStats.xml", Instance);

		internal static void Reload() => _instance = new Lazy<DefaultDeckStats>(Load);
	}
}
