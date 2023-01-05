using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum SubFranchise {

		[JsonProperty("Arena")]
		Arena,

		[JsonProperty("Brawl")]
		Brawl,

		[JsonProperty("Duels")]
		Duels,
	}
}
