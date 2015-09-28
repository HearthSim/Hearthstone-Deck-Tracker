#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	[XmlRoot(ElementName = "Decks")]
	public class DeckList
	{
		private static DeckList _instance;
		private Deck _activeDeck;

		[XmlArray(ElementName = "Tags")]
		[XmlArrayItem(ElementName = "Tag")]
		public List<string> AllTags;

		[XmlElement(ElementName = "Deck")]
		public ObservableCollection<Deck> Decks;

		public List<DeckInfo> LastDeckClass;

		public DeckList()
		{
			Decks = new ObservableCollection<Deck>();
			AllTags = new List<string>();
		}

		[XmlIgnore]
		public Deck ActiveDeck
		{
			get { return _activeDeck; }
			set
			{
				if(Equals(_activeDeck, value))
					return;
				_activeDeck = value;
				Core.MainWindow.DeckPickerList.ActiveDeckChanged();
				Core.MainWindow.DeckPickerList.RefreshDisplayedDecks();
				Logger.WriteLine("Set active deck to: " + value, "DeckList");
				Config.Instance.ActiveDeckId = value == null ? Guid.Empty : value.DeckId;
				Config.Save();
			}
		}

		public Deck ActiveDeckVersion
		{
			get { return ActiveDeck == null ? null : ActiveDeck.GetSelectedDeckVersion(); }
		}

	    public static DeckList Instance
	    {
	        get
	        {
	            if (_instance == null)
	                Load();
	            return _instance ?? (_instance = new DeckList());
	        }
	    }

	    public void LoadActiveDeck()
	    {
	        var deck = Decks.FirstOrDefault(d => d.DeckId == Config.Instance.ActiveDeckId);
	        if (deck != null && deck.Archived)
	            deck = null;
	        ActiveDeck = deck;
	    }

	    //public Guid ActiveDeckId { get; set; }

		public static void Load()
		{
            SetupDeckListFile();
			var file = Config.Instance.DataDir + "PlayerDecks.xml";
			if(!File.Exists(file))
				return;
			try
			{
				_instance = XmlManager<DeckList>.Load(file);
			}
			catch(Exception)
			{
				//failed loading deckstats 
				var corruptedFile = Helper.GetValidFilePath(Config.Instance.DataDir, "PlayerDecks_corrupted", "xml");
				try
				{
					File.Move(file, corruptedFile);
				}
				catch(Exception)
				{
					throw new Exception(
						"Can not load or move PlayerDecks.xml file. Please manually delete the file in \"%appdata\\HearthstoneDeckTracker\".");
				}

				//get latest backup file
				var backup =
					new DirectoryInfo(Config.Instance.DataDir).GetFiles("PlayerDecks_backup*").OrderByDescending(x => x.CreationTime).FirstOrDefault();
				if(backup != null)
				{
					try
					{
						File.Copy(backup.FullName, file);
						_instance = XmlManager<DeckList>.Load(file);
					}
					catch(Exception ex)
					{
						throw new Exception(
							"Error restoring PlayerDecks backup. Please manually rename \"PlayerDecks_backup.xml\" to \"PlayerDecks.xml\" in \"%appdata\\HearthstoneDeckTracker\".",
							ex);
					}
				}
				else
					throw new Exception("PlayerDecks.xml is corrupted.");
			}

			var save = false;
			if(!Instance.AllTags.Contains("All"))
			{
				Instance.AllTags.Add("All");
				save = true;
			}
			if(!Instance.AllTags.Contains("Favorite"))
			{
				if(Instance.AllTags.Count > 1)
					Instance.AllTags.Insert(1, "Favorite");
				else
					Instance.AllTags.Add("Favorite");
				save = true;
			}
			if(!Instance.AllTags.Contains("None"))
			{
				Instance.AllTags.Add("None");
				save = true;
			}
			if(save)
				Save();

			Instance.LoadActiveDeck();
		}

        internal static void SetupDeckListFile()
        {
            if(Config.Instance.SaveDataInAppData == null)
                return;
            var appDataPath = Config.Instance.AppDataPath + @"\PlayerDecks.xml";
            var dataDirPath = Config.Instance.DataDirPath + @"\PlayerDecks.xml";
            if(Config.Instance.SaveDataInAppData.Value)
            {
                if(File.Exists(dataDirPath))
                {
                    if(File.Exists(appDataPath))
                        //backup in case the file already exists
                        File.Move(appDataPath, appDataPath + DateTime.Now.ToFileTime());
                    File.Move(dataDirPath, appDataPath);
                    Logger.WriteLine("Moved decks to appdata", "Load");
                }
            }
            else if(File.Exists(appDataPath))
            {
                if(File.Exists(dataDirPath))
                    //backup in case the file already exists
                    File.Move(dataDirPath, dataDirPath + DateTime.Now.ToFileTime());
                File.Move(appDataPath, dataDirPath);
                Logger.WriteLine("Moved decks to local", "Load");
            }

            //create file if it doesn't exist
            var path = Path.Combine(Config.Instance.DataDir, "PlayerDecks.xml");
            if(!File.Exists(path))
            {
                using(var sr = new StreamWriter(path, false))
                    sr.WriteLine("<Decks></Decks>");
            }
        }

        public static void Save()
		{
			var file = Config.Instance.DataDir + "PlayerDecks.xml";
			XmlManager<DeckList>.Save(file, Instance);
		}
	}

	public class DeckInfo
	{
		public string Class;
		public Guid Id;
		public string Name;
	}
}