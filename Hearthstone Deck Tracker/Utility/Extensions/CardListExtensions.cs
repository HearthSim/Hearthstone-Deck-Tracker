using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public static class CardListExtensions
	{
		public enum CardSorting
		{
			Cost,
			MulliganWr,
		}

		public static List<Card> ToSortedCardList(this IEnumerable<Card> cards, CardSorting sorting = CardSorting.Cost)
		{
			var orderedCards = sorting == CardSorting.MulliganWr
				? cards.OrderByDescending(x => x.CardWinrates?.MulliganWinrate)
				: cards.OrderByDescending(x => x.HideStats);
			return orderedCards
				.ThenBy(x => x.Cost)
				.ThenBy(x => x.LocalizedName)
				.ThenBy(x => x.ExtraInfo?.CardNameSuffix)
				.ToList();
		}

		public static List<Card> ToDiffCardList(this IEnumerable<Card> cards, List<Card> newCards)
		{
			var diff = new List<Card>();

			//removed
			foreach(var c in cards.Where(c => !newCards.Contains(c)))
			{
				var cd = c.Clone() as Card;
				if(cd == null)
					continue;
				cd.Count = -cd.Count; //mark as negative for visual
				diff.Add(cd);
			}

			//added
			diff.AddRange(newCards.Where(c => !cards.Contains(c)));

			//diff count
			var diffCount =
				newCards.Where(c => cards.Any(c2 => c2.Id == c.Id) && cards.First(c2 => c2.Id == c.Id).Count != c.Count);
			foreach(var card in diffCount)
			{
				var cardclone = card.Clone() as Card;
				if(cardclone == null)
					continue;
				cardclone.Count -= cards.First(c => c.Id == cardclone.Id).Count;
				diff.Add(cardclone);
			}

			return diff;
		}

		public static void AddCard(this List<Card> cards, Card newCard)
		{
			var existingCard = cards.FirstOrDefault(c => c.Id == newCard.Id);
			if (existingCard != null)
			{
				existingCard.Count += newCard.Count;
			}
			else
			{
				cards.Add(newCard);
			}

		}

		public static void AddCardRange(this List<Card> cards, IEnumerable<Card> newCards)
		{
			foreach (var card in newCards)
			{
				cards.AddCard(card);
			}
		}

		public static List<Card> ConcatCardList(this List<Card> cards, IEnumerable<Card> newCards)
		{
			cards.AddCardRange(newCards);
			return cards;
		}

		private const GameTag GalakrondTag = (GameTag)3623;
		private static readonly HashSet<string> UngeneratableCards = new()
		{
			// Deck-rule legendaries
			HearthDb.CardIds.Collectible.Neutral.PrinceRenathal,
			HearthDb.CardIds.Collectible.Neutral.PrinceRenathalCorePlaceholder,
			HearthDb.CardIds.Collectible.Neutral.GennGreymane,
			HearthDb.CardIds.Collectible.Neutral.GennGreymaneCorePlaceholder,
			HearthDb.CardIds.Collectible.Neutral.BakuTheMooneater,
			HearthDb.CardIds.Collectible.Neutral.BakuTheMooneaterCorePlaceholder,
			HearthDb.CardIds.Collectible.Priest.DarkbishopBenedictus,
			HearthDb.CardIds.Collectible.Priest.DarkbishopBenedictusCorePlaceholder,
			HearthDb.CardIds.Collectible.Neutral.WhizbangTheWonderful,
			HearthDb.CardIds.Collectible.Neutral.SplendiferousWhizbang,
			HearthDb.CardIds.Collectible.Neutral.ZayleShadowCloak,
			// Start of Game / start-in-hand or -deck legendaries
			HearthDb.CardIds.Collectible.Neutral.NozdormuTheEternalCore,
			HearthDb.CardIds.Collectible.Neutral.ChogallTwilightChieftain,
			HearthDb.CardIds.Collectible.Neutral.PrinceMalchezaar,
			HearthDb.CardIds.Collectible.Rogue.MaestraOfTheMasquerade,
			HearthDb.CardIds.Collectible.Demonhunter.SouleatersScythe,
			HearthDb.CardIds.Collectible.Neutral.CthunTheShattered,
			HearthDb.CardIds.Collectible.Neutral.DragonSoulShattered,
			HearthDb.CardIds.Collectible.Druid.HamuulRunetotem,
			HearthDb.CardIds.Collectible.Warrior.SporeEmpressMoldara,
			// Galakronds
			HearthDb.CardIds.Collectible.Warlock.GalakrondTheWretched,
			HearthDb.CardIds.Collectible.Rogue.GalakrondTheNightmare,
			HearthDb.CardIds.Collectible.Shaman.GalakrondTheTempest,
			HearthDb.CardIds.Collectible.Warrior.GalakrondTheUnbreakable,
			HearthDb.CardIds.Collectible.Priest.GalakrondTheUnspeakable,
			// Tourists
			HearthDb.CardIds.Collectible.Rogue.MaestraMaskMerchant,
			HearthDb.CardIds.Collectible.Warrior.HammTheHungry,
			HearthDb.CardIds.Collectible.Hunter.RangerGilly,
			HearthDb.CardIds.Collectible.Mage.RayllaSandSculptor,
			HearthDb.CardIds.Collectible.Deathknight.Buttons,
			HearthDb.CardIds.Collectible.Shaman.CarefreeCookie,
			HearthDb.CardIds.Collectible.Demonhunter.ArannaThrillSeeker,
			HearthDb.CardIds.Collectible.Warlock.SummonerDarkmarrow,
			HearthDb.CardIds.Collectible.Paladin.SunsapperLynessa,
			HearthDb.CardIds.Collectible.Druid.MistahVistah,
			HearthDb.CardIds.Collectible.Priest.ChillinVoljin,
			HearthDb.CardIds.Collectible.Shaman.Turbulus,
			HearthDb.CardIds.Collectible.Mage.PortalmancerSkyla,
			// Other Non-generated
			HearthDb.CardIds.Collectible.Rogue.BounceAroundFtGarona,
			HearthDb.CardIds.Collectible.Rogue.SliceAndDice,
		};
		// some cards are only available in pools if the deck has some requirements, such as Imbue or Herald
		public static IEnumerable<Card> FilterGenerationPool(this IEnumerable<Card> cards, List<Card> deck)
		{
			var containsImbue = deck.Exists(c => c.HasTag(GameTag.IMBUE));
			var containsGalakrond = deck.Exists(c => c.HasTag(GalakrondTag));
			var containsHerald = deck.Exists(c => c.HasTag(GameTag.HERALD));
			var containsExcavate = deck.Exists(c => c.HasTag(GameTag.EXCAVATE));
			var containsZerg = deck.Exists(c => c.HasTag(GameTag.ZERG));
			var containsTerran = deck.Exists(c => c.HasTag(GameTag.TERRAN));
			var containsProtoss = deck.Exists(c => c.HasTag(GameTag.PROTOSS));

			return cards.Where(c =>
				IsAllowed(
					c,
					containsImbue,
					containsGalakrond,
					containsHerald,
					containsExcavate,
					containsZerg,
					containsTerran,
					containsProtoss
				)
			);

			bool IsAllowed(
				Card card,
				bool deckHasImbue,
				bool deckHasGalakrond,
				bool deckHasHerald,
				bool deckHasExcavate,
				bool deckHasZerg,
				bool deckHasTerran,
				bool deckHasProtoss)
			{
				if(card.GetTag(GameTag.COST_BLOOD) == 3 || card.GetTag(GameTag.COST_FROST) == 3
				                                        || card.GetTag(GameTag.COST_UNHOLY) == 3)
					return false;

				if(card.HasTag(GameTag.FABLED) || card.HasTag(GameTag.FABLED_PLUS))
					return false;

				if (card.HasTag(GameTag.COLOSSAL) || card.HasTag(GameTag.TITAN) || card.HasTag(GameTag.QUEST) || card.HasTag(GameTag.QUESTLINE))
					return false;

				if(card.HasTag(GameTag.SIDEBOARD_TYPE) || UngeneratableCards.Contains(card.Id))
					return false;

				if (card.HasTag(GameTag.IMBUE) && !deckHasImbue)
					return false;

				if (card.HasTag(GalakrondTag) && !deckHasGalakrond)
					return false;

				if (card.HasTag(GameTag.HERALD) && !deckHasHerald)
					return false;

				if (card.HasTag(GameTag.EXCAVATE) && !deckHasExcavate)
					return false;

				if(card.HasTag(GameTag.ZERG) && !deckHasZerg)
					return false;

				if(card.HasTag(GameTag.TERRAN) && !deckHasTerran)
					return false;

				if(card.HasTag(GameTag.PROTOSS) && !deckHasProtoss)
					return false;

				return true;
			}
		}

	}
}
