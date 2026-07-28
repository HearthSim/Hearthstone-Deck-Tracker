using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Cast 5 random Secrets from the past. At the start of your turn, destroy them."
// Secrets in play must be distinct, so the 5 casts are modeled as one batch of 5 unique
// draws (without replacement).
public class DiscoAtTheEndOfTime : FromThePastPoolCard, ICardGenerator
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.DiscoAtTheEndOfTime;
	public override int Picks() => 5;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL } && c.HasTag(GameTag.SECRET))
			.Select(c => new Card(c));
	}

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.GetTag(GameTag.SECRET) > 0 &&
		       (Helper.WildOnlySets.Contains(card.Set) ||
		        Helper.ClassicOnlySets.Contains(card.Set));
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.All(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}
