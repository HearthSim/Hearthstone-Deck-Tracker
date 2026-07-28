using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "After you cast a Fel spell, get a random non-Colossal Naga."
public class NespirahUnshackledToken : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.NespirahEnthralled_NespirahUnshackledToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsNaga() && !c.HasTag(GameTag.COLOSSAL))
			.Select(c => new Card(c));
	}
}
