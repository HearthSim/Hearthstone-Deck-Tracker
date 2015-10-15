using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class ConfigWrapper
	{
		public static bool ArenaStatsShowLegends
		{
			get { return Config.Instance.ArenaStatsShowLegends; }
			set
			{
				Config.Instance.ArenaStatsShowLegends = value;
				Config.Save();
			}
		}
	}
}
