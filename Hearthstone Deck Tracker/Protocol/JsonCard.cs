#region

using Hearthstone_Deck_Tracker.Hearthstone;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.Protocol
{
	[JsonObject]
	public class JsonCard
	{
		[JsonProperty("count")]
		public int Count = 1;

		[JsonProperty("id")]
		public string Id = "";

		[JsonProperty("name")]
		public string Name = "";

		public Card ToCard(bool localizedName)
		{
			Card card = null;
			if(!string.IsNullOrEmpty(Id))
				card = Database.GetCardFromId(Id);
			else if(!string.IsNullOrEmpty(Name))
				card = Database.GetCardFromName(Name, localizedName);
			if(card != null)
				card.Count = Count;
			return card;
		}
	}
}