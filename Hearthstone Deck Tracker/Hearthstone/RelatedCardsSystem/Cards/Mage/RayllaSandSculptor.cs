using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Paladin Tourist After you cast a spell, summon a random 2-Cost minion and give it Divine Shield."
public class RayllaSandSculptor : Cost2MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.RayllaSandSculptor;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
