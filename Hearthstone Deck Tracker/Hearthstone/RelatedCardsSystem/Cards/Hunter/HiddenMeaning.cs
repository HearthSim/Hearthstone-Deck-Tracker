namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Secret: When your opponent ends their turn with no Mana, summon a random 3-Cost minion."
public class HiddenMeaning : WanderingMonster
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.HiddenMeaning;
}
