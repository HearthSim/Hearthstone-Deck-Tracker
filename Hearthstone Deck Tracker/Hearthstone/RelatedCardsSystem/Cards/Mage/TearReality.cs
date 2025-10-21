using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class TearReality : ICardGenerator
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.TearReality;

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.Type == "Spell" && card.IsClass("Mage") &&
			(Helper.WildOnlySets.Contains(card.Set) ||
		        Helper.ClassicOnlySets.Contains(card.Set));
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.All(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}
