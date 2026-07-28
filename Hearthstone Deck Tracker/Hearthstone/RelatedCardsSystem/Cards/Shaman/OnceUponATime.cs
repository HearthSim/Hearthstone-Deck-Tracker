using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Summon a random 3-Cost Beast, Dragon, Elemental, and Murloc."
public class OnceUponATime : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.OnceUponATime;
	public override int Picks() => 1;
	public override int EventCount() => 4;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		var beasts = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: 3 } && c.IsBeast())
			.Select(c => new Card(c));
		var dragons = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: 3 } && c.IsDragon())
			.Select(c => new Card(c));
		var elementals = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: 3 } && c.IsElemental())
			.Select(c => new Card(c));
		var murlocs = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: 3 } && c.IsMurloc())
			.Select(c => new Card(c));
		return beasts.Concat(dragons).Concat(elementals).Concat(murlocs);
	}
}
