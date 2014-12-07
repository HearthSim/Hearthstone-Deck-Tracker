using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

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
			get { return _instance ?? (_instance = new DeckStatsList()); }
		}

		public static void Load()
		{
			var file = Config.Instance.DataDir + "DeckStats.xml";
			try
			{
				_instance = XmlManager<DeckStatsList>.Load(file);
			}
			catch (Exception)
			{
				//failed loading deckstats 
				var corruptedFile = Helper.GetValidFilePath(Config.Instance.DataDir, "DeckStats_corrupted", "xml");
				try
				{
					File.Move(file, corruptedFile);
				}
				catch (Exception)
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
					catch (Exception)
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

		public static void Save()
		{
			var file = Config.Instance.DataDir + "DeckStats.xml";
			XmlManager<DeckStatsList>.Save(file, Instance);
		}

	}
}
