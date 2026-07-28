using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Add a random Legendary minion from the past to your hand."
public class TokiTimeTinker : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.TokiTimeTinker;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Rarity: Rarity.LEGENDARY })
			.Select(c => new Card(c));
	}
}
