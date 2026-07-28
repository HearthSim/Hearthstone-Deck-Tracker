using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Whenever your weapon is destroyed, equip a random Demon Hunter weapon."
public class InstrumentSmasher : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.InstrumentSmasher;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.WEAPON } && c.IsClass("DemonHunter"))
			.Select(c => new Card(c));
	}
}
