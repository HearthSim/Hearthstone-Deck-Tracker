using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Add 2 random Mage spells from the past to your hand. They cost (2) less."
public class TearReality : FromThePastPoolCard, ICardGenerator
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.TearReality;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL } && c.IsClass("Mage"))
			.Select(c => new Card(c));
	}

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.TypeEnum == CardType.SPELL && card.IsClass("Mage") &&
			(Helper.WildOnlySets.Contains(card.Set) ||
		        Helper.ClassicOnlySets.Contains(card.Set));
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.All(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}
