using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;

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

		public DeckStats GetDeckStats(string hero)
		{
			if(!Game.Classes.Contains(hero))
				return null;
			var ds = DeckStats.FirstOrDefault(d => d.Name == hero);
			if(ds == null)
			{
				ds = new DeckStats(hero);
				DeckStats.Add(ds);
			}
			return ds;
		}

		public static DefaultDeckStats Instance
		{
			get { return _instance ?? (_instance = new DefaultDeckStats()); }
		}

		public static void Load()
		{
			var file = Config.Instance.DataDir + "DefaultDeckStats.xml";
			_instance = XmlManager<DefaultDeckStats>.Load(file);
		}

		public static void Save()
		{
			var file = Config.Instance.DataDir + "DefaultDeckStats.xml";
			XmlManager<DefaultDeckStats>.Save(file, Instance);
		}
	}
}
