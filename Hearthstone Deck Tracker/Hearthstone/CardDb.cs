#region

using System.Collections.Generic;
using System.Xml.Serialization;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	[XmlRoot(ElementName = "CardDb")]
	public class CardDb
	{
		[XmlElement(ElementName = "Card")]
		public List<CardDbObj> Cards { get; set; }
	}

	public class CardDbObj
	{
		public string CardId { get; set; }
		public string Name { get; set; }
		public string CardSet { get; set; }
		public string Rarity { get; set; }
		public string Type { get; set; }
		public int Attack { get; set; }
		public int Health { get; set; }
		public int Cost { get; set; }
		public int Durability { get; set; }
		public string Class { get; set; }
		public string Faction { get; set; }
		public string Race { get; set; }
		public string Text { get; set; }
		public string[] Mechanics { get; set; }
		public string Artist { get; set; }

		public Card ToCard()
		{
			return new Card
			{
				Id = CardId,
				Artist = Artist,
				Attack = Attack,
				Cost = Cost,
				Durability = (Durability > 0 ? (int?)Durability : null),
				Health = Health,
				Name = Name,
				LocalizedName = Name,
				Mechanics = Mechanics,
				Race = Race,
				Set = CardSet,
				Text = Text,
				Type = Type,
				Rarity = Rarity,
				PlayerClass = Class
			};
		}
	}
}