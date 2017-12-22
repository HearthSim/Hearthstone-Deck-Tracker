#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Stats
{
	public class DeckStatsList
	{
		private static Lazy<DeckStatsList> _instance = new Lazy<DeckStatsList>(Load);

		[XmlArray(ElementName = "DeckStats")]
		[XmlArrayItem(ElementName = "Deck")]
		public List<DeckStats> SerializableDeckStats = new List<DeckStats>();

		private ConcurrentDictionary<Guid, DeckStats> _deckStats;
		[XmlIgnore]
		public ConcurrentDictionary<Guid, DeckStats> DeckStats => _deckStats ?? (_deckStats =
																	new ConcurrentDictionary<Guid, DeckStats>(
																		SerializableDeckStats.Where(x => x != null).GroupBy(x => x.DeckId).Select(x => new KeyValuePair<Guid, DeckStats>(x.First().DeckId, x.First()))));

		public static DeckStatsList Instance => _instance.Value;

		private static DeckStatsList Load()
		{
#if(!SQUIRREL)
			SetupDeckStatsFile();
#endif
			var file = Path.Combine(Config.Instance.DataDir, "DeckStats.xml");
			if(!File.Exists(file))
				return new DeckStatsList();
			DeckStatsList instance = null;
			try
			{
				instance = XmlManager<DeckStatsList>.Load(file);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				try
				{
					File.Move(file, Helper.GetValidFilePath(Config.Instance.DataDir, "DeckStats_corrupted", "xml"));
				}
				catch(Exception ex1)
				{
					Log.Error(ex1);
				}
				instance = BackupManager.TryRestore<DeckStatsList>("DeckStats.xml") ?? new DeckStatsList();
			}
			return instance;
		}

#if(!SQUIRREL)
		internal static void SetupDeckStatsFile()
		{
			if(Config.Instance.SaveDataInAppData == null)
				return;
			var appDataPath = Config.AppDataPath + @"\DeckStats.xml";
			var dataDirPath = Config.Instance.DataDirPath + @"\DeckStats.xml";
			if(Config.Instance.SaveDataInAppData.Value)
			{
				if(File.Exists(dataDirPath))
				{
					if(File.Exists(appDataPath))
					{
						//backup in case the file already exists
						var time = DateTime.Now.ToFileTime();
						File.Move(appDataPath, appDataPath + time);
						Log.Info("Created backups of DeckStats and Games in appdata");
					}
					File.Move(dataDirPath, appDataPath);
					Log.Info("Moved DeckStats to appdata");
				}
			}
			else if(File.Exists(appDataPath))
			{
				if(File.Exists(dataDirPath))
				{
					//backup in case the file already exists
					var time = DateTime.Now.ToFileTime();
					File.Move(dataDirPath, dataDirPath + time);
					Log.Info("Created backups of deckstats and games locally");
				}
				File.Move(appDataPath, dataDirPath);
				Log.Info("Moved DeckStats to local");
			}
		}
#endif


		public static void Save()
		{
			Instance.SerializableDeckStats = Instance.DeckStats.Values.ToList();
			XmlManager<DeckStatsList>.Save(Config.Instance.DataDir + "DeckStats.xml", Instance);
		}

		internal static void Reload() => _instance = new Lazy<DeckStatsList>(Load);

		internal DeckStats Add(Deck deck)
		{
			var ds = new DeckStats(deck);
			Instance.DeckStats.TryAdd(deck.DeckId, ds);
			return ds;
		}
	}
}
