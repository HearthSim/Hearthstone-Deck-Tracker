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
	        get
	        {
	            if (_instance == null)
	                Load();
	            return _instance ?? (_instance = new DefaultDeckStats());
	        }
	    }

	    public DeckStats GetDeckStats(string hero)
		{
			//if(!Enum.GetNames(typeof(HeroClass)).Contains(hero))
			//	return null;
		    if(string.IsNullOrEmpty(hero))
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
            SetupDefaultDeckStatsFile();
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


        internal static void SetupDefaultDeckStatsFile()
        {
            if(Config.Instance.SaveDataInAppData == null)
                return;
            var appDataPath = Config.Instance.AppDataPath + @"\DefaultDeckStats.xml";
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
                        Logger.WriteLine("Created backups of DefaultDeckStats in appdata", "Load");
                    }
                    File.Move(dataDirPath, appDataPath);
                    Logger.WriteLine("Moved DefaultDeckStats to appdata", "Load");
                }
            }
            else if(File.Exists(appDataPath))
            {
                if(File.Exists(dataDirPath))
                {
                    //backup in case the file already exists
                    var time = DateTime.Now.ToFileTime();
                    File.Move(dataDirPath, dataDirPath + time);
                    Logger.WriteLine("Created backups of DefaultDeckStats locally", "Load");
                }
                File.Move(appDataPath, dataDirPath);
                Logger.WriteLine("Moved DefaultDeckStats to local", "Load");
            }

            var filePath = Config.Instance.DataDir + "DefaultDeckStats.xml";
            //create if it does not exist
            if(!File.Exists(filePath))
            {
                using(var sr = new StreamWriter(filePath, false))
                    sr.WriteLine("<DefaultDeckStats></DefaultDeckStats>");
            }
        }


        public static void Save()
		{
			var file = Config.Instance.DataDir + "DefaultDeckStats.xml";
			XmlManager<DefaultDeckStats>.Save(file, Instance);
		}
	}
}