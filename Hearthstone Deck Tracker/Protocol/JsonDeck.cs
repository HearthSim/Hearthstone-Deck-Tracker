using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Protocol
{
	[JsonObject]
	public class JsonDeck
	{
		[JsonProperty("name")]
		public string Name = "";

		[JsonProperty("cards")]
		public JsonCard[] Cards = {};

		[JsonProperty("tags")]
		public string[] Tags = {};

		[JsonProperty("url")]
		public string Url = "";

		[JsonProperty("arena")]
		public bool IsArena;

		[JsonProperty("nonenglish")]
		public bool LocalizedNames;

		public Deck ToDeck()
		{
			var deck = new Deck {Name = Name, Url = Url, LastEdited = DateTime.Now};
			if(IsArena)
				deck.IsArenaDeck = true;
			foreach(var card in Cards.Select(x => x.ToCard(LocalizedNames)).Where(c => c != null))
			{
				if(string.IsNullOrEmpty(deck.Class) && !string.IsNullOrEmpty(card.PlayerClass))
					deck.Class = card.PlayerClass;
				deck.Cards.Add(card);
			}
			foreach(var tag in Tags)
				deck.Tags.Add(tag);
			return deck;
		}
	}
}
