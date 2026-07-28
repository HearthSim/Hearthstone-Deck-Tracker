using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Discover and cast a Secret. Forge: Do it twice."
public class TitanforgedTraps : ClassOrNeutralSecretPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.TitanforgedTraps;
}

// "Forged Discover and cast two Secrets."
public class TitanforgedTrapsToken : TitanforgedTraps
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Hunter.TitanforgedTraps_TitanforgedTrapsToken;
	public override int EventCount() => 2;
}
