using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards;

// Shared base for cards that trigger one of C'Thun, Eye of the Empire's abilities.
public abstract class CthunAbilityCard : ICardWithRelatedCards
{
	public abstract string GetCardId();

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.CThunEyeoftheEmpire_WrathOfCthunToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.CThunEyeoftheEmpire_DomainOfCthunToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.CThunEyeoftheEmpire_GlareOfCthunToken1),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.CThunEyeoftheEmpire_ServantsOfCthunToken),
		};
}
