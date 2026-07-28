using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Charge After this attacks, Discover a card from another class. It costs (3) less."
// Standard Discover sampling.
public class VelarokTheDeceiverToken : OffClassCardPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Rogue.VelarokWindblade_VelarokTheDeceiverToken;
	public override int Picks() => 3;
	public override bool IsWithReplacement() => false;
}
