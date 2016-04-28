#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Stats
{
	public class DeckStatsList
	{
		private static Lazy<DeckStatsList> _instance = new Lazy<DeckStatsList>(Load);

		[XmlArray(ElementName = "DeckStats")]
		[XmlArrayItem(ElementName = "Deck")]
		public List<DeckStats> DeckStats;

		public DeckStatsList()
		{
			DeckStats = new List<DeckStats>();
		}

		public static DeckStatsList Instance => _instance.Value;

		private static DeckStatsList Load()
		{
			SetupDeckStatsFile();
			var file = Config.Instance.DataDir + "DeckStats.xml";
			if(!File.Exists(file))
				return new DeckStatsList();
			try
			{
				return XmlManager<DeckStatsList>.Load(file);
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
					throw new Exception(
						"Can not load or move DeckStats.xml file. Please manually delete the file in \"%appdata\\HearthstoneDeckTracker\".");
				}

				//get latest backup file
				var backup =
					new DirectoryInfo(Config.Instance.DataDir).GetFiles("DeckStats_backup*").OrderByDescending(x => x.CreationTime).FirstOrDefault();
				if(backup != null)
				{
					try
					{
						File.Copy(backup.FullName, file);
						return XmlManager<DeckStatsList>.Load(file);
					}
					catch(Exception ex)
					{
						throw new Exception(
							"Error restoring DeckStats backup. Please manually rename \"DeckStats_backup.xml\" to \"DeckStats.xml\" in \"%appdata\\HearthstoneDeckTracker\".",
							ex);
					}
				}
				throw new Exception("DeckStats.xml is corrupted.");
			}
		}

		internal static void SetupDeckStatsFile()
		{
			if(Config.Instance.SaveDataInAppData == null)
				return;
			var appDataPath = Config.AppDataPath + @"\DeckStats.xml";
			var appDataGamesDirPath = Config.AppDataPath + @"\Games";
			var dataDirPath = Config.Instance.DataDirPath + @"\DeckStats.xml";
			var dataGamesDirPath = Config.Instance.DataDirPath + @"\Games";
			if(Config.Instance.SaveDataInAppData.Value)
			{
				if(File.Exists(dataDirPath))
				{
					if(File.Exists(appDataPath))
					{
						//backup in case the file already exists
						var time = DateTime.Now.ToFileTime();
						File.Move(appDataPath, appDataPath + time);
						if(Directory.Exists(appDataGamesDirPath))
						{
							Helper.CopyFolder(appDataGamesDirPath, appDataGamesDirPath + time);
							Directory.Delete(appDataGamesDirPath, true);
						}
						Log.Info("Created backups of DeckStats and Games in appdata");
					}
					File.Move(dataDirPath, appDataPath);
					Log.Info("Moved DeckStats to appdata");
					if(Directory.Exists(dataGamesDirPath))
					{
						Helper.CopyFolder(dataGamesDirPath, appDataGamesDirPath);
						Directory.Delete(dataGamesDirPath, true);
					}
					Log.Info("Moved Games to appdata");
				}
			}
			else if(File.Exists(appDataPath))
			{
				if(File.Exists(dataDirPath))
				{
					//backup in case the file already exists
					var time = DateTime.Now.ToFileTime();
					File.Move(dataDirPath, dataDirPath + time);
					if(Directory.Exists(dataGamesDirPath))
					{
						Helper.CopyFolder(dataGamesDirPath, dataGamesDirPath + time);
						Directory.Delete(dataGamesDirPath, true);
					}
					Log.Info("Created backups of deckstats and games locally");
				}
				File.Move(appDataPath, dataDirPath);
				Log.Info("Moved DeckStats to local");
				if(Directory.Exists(appDataGamesDirPath))
				{
					Helper.CopyFolder(appDataGamesDirPath, dataGamesDirPath);
					Directory.Delete(appDataGamesDirPath, true);
				}
				Log.Info("Moved Games to appdata");
			}

			var filePath = Config.Instance.DataDir + "DeckStats.xml";
			//create file if it does not exist
			if(!File.Exists(filePath))
			{
				using(var sr = new StreamWriter(filePath, false))
					sr.WriteLine("<DeckStatsList></DeckStatsList>");
			}
		}


		public static void Save() => XmlManager<DeckStatsList>.Save(Config.Instance.DataDir + "DeckStats.xml", Instance);

		internal static void Reload() => _instance = new Lazy<DeckStatsList>(Load);
	}
}