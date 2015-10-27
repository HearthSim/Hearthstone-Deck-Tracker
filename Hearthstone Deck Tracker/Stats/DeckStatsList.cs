#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

#endregion

namespace Hearthstone_Deck_Tracker.Stats
{
	public class DeckStatsList
	{
		private static DeckStatsList _instance;

		[XmlArray(ElementName = "DeckStats")]
		[XmlArrayItem(ElementName = "Deck")]
		public List<DeckStats> DeckStats;

		public DeckStatsList()
		{
			DeckStats = new List<DeckStats>();
		}

	    public static DeckStatsList Instance
	    {
	        get
	        {
	            if (_instance == null)
	                Load();
	            return _instance ?? (_instance = new DeckStatsList());
	        }
	    }

	    public static void Load()
		{
            SetupDeckStatsFile();
			var file = Config.Instance.DataDir + "DeckStats.xml";
			if(!File.Exists(file))
				return;
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
						_instance = XmlManager<DeckStatsList>.Load(file);
					}
					catch(Exception ex)
					{
						throw new Exception(
							"Error restoring DeckStats backup. Please manually rename \"DeckStats_backup.xml\" to \"DeckStats.xml\" in \"%appdata\\HearthstoneDeckTracker\".",
							ex);
					}
				}
				else
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
                        Logger.WriteLine("Created backups of DeckStats and Games in appdata", "Load");
                    }
                    File.Move(dataDirPath, appDataPath);
                    Logger.WriteLine("Moved DeckStats to appdata", "Load");
                    if(Directory.Exists(dataGamesDirPath))
                    {
                        Helper.CopyFolder(dataGamesDirPath, appDataGamesDirPath);
                        Directory.Delete(dataGamesDirPath, true);
                    }
                    Logger.WriteLine("Moved Games to appdata", "Load");
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
                    Logger.WriteLine("Created backups of deckstats and games locally", "Load");
                }
                File.Move(appDataPath, dataDirPath);
                Logger.WriteLine("Moved DeckStats to local", "Load");
                if(Directory.Exists(appDataGamesDirPath))
                {
                    Helper.CopyFolder(appDataGamesDirPath, dataGamesDirPath);
                    Directory.Delete(appDataGamesDirPath, true);
                }
                Logger.WriteLine("Moved Games to appdata", "Load");
            }

            var filePath = Config.Instance.DataDir + "DeckStats.xml";
            //create file if it does not exist
            if(!File.Exists(filePath))
            {
                using(var sr = new StreamWriter(filePath, false))
                    sr.WriteLine("<DeckStatsList></DeckStatsList>");
            }
        }


        public static void Save()
		{
			var file = Config.Instance.DataDir + "DeckStats.xml";
			XmlManager<DeckStatsList>.Save(file, Instance);
		}
	}
}