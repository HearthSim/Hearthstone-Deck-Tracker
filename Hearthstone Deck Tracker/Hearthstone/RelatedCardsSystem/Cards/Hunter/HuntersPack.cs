using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Add a random Hunter Beast, Secret, and weapon to your hand."
public class HuntersPack : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.HuntersPack;
	public override int Picks() => 1;
	public override int EventCount() => 3;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		var beasts = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsClass("Hunter") && c.IsBeast())
			.Select(c => new Card(c));
		var secrets = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL } && c.IsClass("Hunter") && c.HasTag(GameTag.SECRET))
			.Select(c => new Card(c));
		var weapons = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.WEAPON } && c.IsClass("Hunter"))
			.Select(c => new Card(c));
		return beasts.Concat(secrets).Concat(weapons);
	}
}
