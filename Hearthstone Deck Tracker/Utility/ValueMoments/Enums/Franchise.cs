using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum Franchise {

		[JsonProperty("All")]
		All,

		[JsonProperty("HS-Constructed")]
		HSConstructed,

		[JsonProperty("Battlegrounds")]
		Battlegrounds,

		[JsonProperty("Mercenaries")]
		Mercenaries,
	}
}
