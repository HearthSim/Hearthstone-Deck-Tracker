#region

using System;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API.Objects
{
	public class CardObject
	{
		public string count;
		public string id;

		public CardObject(Card card)
		{
			if(card != null)
			{
				id = card.Id;
				count = card.Count.ToString();
			}
		}

		public Card ToCard()
		{
			try
			{
				if(string.IsNullOrEmpty(id) || string.IsNullOrEmpty(count))
					return null;
				var card = Game.GetCardFromId(id);
				card.Count = Int32.Parse(count);
				return card;
			}
			catch(Exception e)
			{
				Logger.WriteLine("error converting CardObject: " + e, "HearthStatsAPI");
				return null;
			}
		}
	}
}