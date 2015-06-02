#region

using Hearthstone_Deck_Tracker.Hearthstone;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.Protocol
{
	[JsonObject]
	public class JsonCard
	{
		[JsonProperty("id")]
		public string Id = "";

		[JsonProperty("name")]
		public string Name = "";

		[JsonProperty("count")]
		public int Count = 1;

		public Card ToCard(bool localizedName)
		{
			Card card = null; 
			if(!string.IsNullOrEmpty(Id))
				card = Game.GetCardFromId(Id);
			else if(!string.IsNullOrEmpty(Name))
				card = Game.GetCardFromName(Name, localizedName);
			if(card != null)
				card.Count = Count;
			return card;
		}
	}
}