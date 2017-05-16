using System;
using System.IO;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class DeckSerializer
	{
		public static string Serialize(Deck deck)
		{
			if(!CardIds.HeroNameDict.TryGetValue(deck.Class ?? "", out var heroId))
			{
				Log.Error("Deck has no hero");
				return null;
			}

			var heroDbfId = Database.GetCardFromId(heroId)?.DbfIf ?? 0;
			if(heroDbfId == 0)
			{
				Log.Error("Could not find hero id");
				return null;
			}

			using(var ms = new MemoryStream())
			{
				void Write(int value)
				{
					var bytes = VarInt.GetBytes((ulong)value);
					ms.Write(bytes, 0, bytes.Length);
				}

				ms.WriteByte(0);
				Write(1);
				Write(deck.IsWildDeck ? 1 : 2);
				Write(1);
				Write(heroDbfId);

				var cards = deck.Cards.OrderBy(x => x.DbfIf).ToList();
				var singleCards = cards.Where(x => x.Count == 1).ToList();
				var doubleCards = cards.Where(x => x.Count == 2).ToList();
				var multiCards = cards.Where(x => x.Count > 2).ToList();

				Write(singleCards.Count);
				foreach(var card in singleCards)
					Write(card.DbfIf);

				Write(doubleCards.Count);
				foreach(var card in doubleCards)
					Write(card.DbfIf);

				Write(multiCards.Count);
				foreach(var card in multiCards)
				{
					Write(card.DbfIf);
					Write(card.Count);
				}

				var bytes1 = ms.ToArray();
				return Convert.ToBase64String(bytes1);
			}
		}

		public static Deck Deserialize(string input)
		{
			Deck deck = null;
			var lines = input.Split('\n').Select(x => x.Trim());
			string deckName = null;
			foreach(var line in lines)
			{
				if(string.IsNullOrEmpty(line))
					continue;
				if(line.StartsWith("#"))
				{
					if(line.StartsWith("###"))
						deckName = line.Substring(3).Trim();
					continue;
				}
				try
				{
					if(deck == null)
						deck = DeserializeDeckString(line);
				}
				catch(Exception e)
				{
					Log.Error(e);
					return null;
				}
			}
			if(deck != null && deckName != null)
				deck.Name = deckName;
			return deck;
		}

		public static Deck DeserializeDeckString(string deckString)
		{
			var deck = new Deck();
			byte[] bytes;
			try
			{
				bytes = Convert.FromBase64String(deckString);
			}
			catch(Exception e)
			{
				throw new ArgumentException("Input is not a valid deck string.", e);
			}
			var offset = 0;
			ulong Read()
			{
				if(offset > bytes.Length)
					throw new ArgumentException("Input is not a valid deck string.");
				var value = VarInt.ReadNext(bytes.Skip(offset).ToArray(), out var length);
				offset += length;
				return value;
			}

			//Zero byte
			offset++;

			//Version - currently unused, always 1
			Read();

			//Format - determined dynamically
			Read();

			//Num Heroes - always 1
			Read();

			var heroId = Read();
			var heroCard = Database.GetCardFromDbfId((int)heroId, false);
			deck.Class = heroCard.PlayerClass;

			void AddCard(int? dbfId = null, int count = 1)
			{
				dbfId = dbfId ?? (int)Read();
				var card = Database.GetCardFromDbfId(dbfId.Value);
				card.Count = count;
				deck.Cards.Add(card);
			}

			var numSingleCards = (int)Read();
			for(var i = 0; i < numSingleCards; i++)
				AddCard();

			var numDoubleCards = (int)Read();
			for(var i = 0; i < numDoubleCards; i++)
				AddCard(count: 2);

			var numMultiCards = (int)Read();
			for(var i = 0; i < numMultiCards; i++)
			{
				var dbfId = (int)Read();
				var count = (int)Read();
				AddCard(dbfId, count);
			}
			return deck;
		}
	}
}
