namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: If C'Thun started in your deck, choose one of its abilities to trigger."
public class SpawnOfCthun : CthunAbilityCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.SpawnOfCthun;
}
