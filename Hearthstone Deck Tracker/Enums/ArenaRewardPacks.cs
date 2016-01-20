#region

using System.ComponentModel;

#endregion

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