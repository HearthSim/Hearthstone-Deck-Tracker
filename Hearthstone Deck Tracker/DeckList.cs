#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker
{
	[XmlRoot(ElementName = "Decks")]
	public class DeckList
	{
		private static Lazy<DeckList> _instance = new Lazy<DeckList>(Load);
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
				var switchedDeck = _activeDeck != null;
				_activeDeck = value;
				Core.MainWindow.DeckPickerList.ActiveDeckChanged();
				Core.MainWindow.DeckPickerList.RefreshDisplayedDecks();
				Log.Info("Set active deck to: " + value);
				Config.Instance.ActiveDeckId = value?.DeckId ?? Guid.Empty;
				Config.Save();
				Core.StatsOverview.ConstructedFilters.UpdateActiveDeckOnlyCheckBox();
				Core.StatsOverview.ConstructedGames.UpdateAddGameButton();
				if(switchedDeck)
					Core.StatsOverview.ConstructedSummary.UpdateContent();
			}
		}

		public Deck ActiveDeckVersion => ActiveDeck?.GetSelectedDeckVersion();

		public static DeckList Instance => _instance.Value;

		private void LoadActiveDeck()
		{
			var deck = Decks.FirstOrDefault(d => d.DeckId == Config.Instance.ActiveDeckId);
			if(deck != null && deck.Archived)
				deck = null;
			_activeDeck = deck;
		}

		private static DeckList Load()
		{
			SetupDeckListFile();
			var file = Config.Instance.DataDir + "PlayerDecks.xml";
			if(!File.Exists(file))
				return new DeckList();
			DeckList instance;
			try
			{
				instance = XmlManager<DeckList>.Load(file);
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
						instance = XmlManager<DeckList>.Load(file);
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
			if(!instance.AllTags.Contains("All"))
			{
				instance.AllTags.Add("All");
				save = true;
			}
			if(!instance.AllTags.Contains("Favorite"))
			{
				if(instance.AllTags.Count > 1)
					instance.AllTags.Insert(1, "Favorite");
				else
					instance.AllTags.Add("Favorite");
				save = true;
			}
			if(!instance.AllTags.Contains("None"))
			{
				instance.AllTags.Add("None");
				save = true;
			}
			if(save)
				Save(instance);

			instance.LoadActiveDeck();
			return instance;
		}

		internal static void SetupDeckListFile()
		{
			if(Config.Instance.SaveDataInAppData == null)
				return;
			var appDataPath = Config.AppDataPath + @"\PlayerDecks.xml";
			var dataDirPath = Config.Instance.DataDirPath + @"\PlayerDecks.xml";
			if(Config.Instance.SaveDataInAppData.Value)
			{
				if(File.Exists(dataDirPath))
				{
					if(File.Exists(appDataPath))
						//backup in case the file already exists
						File.Move(appDataPath, appDataPath + DateTime.Now.ToFileTime());
					File.Move(dataDirPath, appDataPath);
					Log.Info("Moved decks to appdata");
				}
			}
			else if(File.Exists(appDataPath))
			{
				if(File.Exists(dataDirPath))
					//backup in case the file already exists
					File.Move(dataDirPath, dataDirPath + DateTime.Now.ToFileTime());
				File.Move(appDataPath, dataDirPath);
				Log.Info("Moved decks to local");
			}

			//create file if it doesn't exist
			var path = Path.Combine(Config.Instance.DataDir, "PlayerDecks.xml");
			if(!File.Exists(path))
			{
				using(var sr = new StreamWriter(path, false))
					sr.WriteLine("<Decks></Decks>");
			}
		}

		private static void Save(DeckList instance) => XmlManager<DeckList>.Save(Config.Instance.DataDir + "PlayerDecks.xml", instance);
		public static void Save() => Save(Instance);

		internal static void Reload() => _instance = new Lazy<DeckList>(Load);
	}

	public class DeckInfo
	{
		public string Class;
		public Guid Id;
		public string Name;
	}
}