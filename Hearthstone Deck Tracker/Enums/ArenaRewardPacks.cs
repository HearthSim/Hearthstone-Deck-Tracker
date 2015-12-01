using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum ArenaRewardPacks
	{
		[Description("None")]
		None,
		[Description("Classic")]
		Classic,
		[Description("Goblins vs Gnomes")]
		GoblinsVsGnomes,
		[Description("The Grand Tournament")]
		TheGrandTournament
	}
}
