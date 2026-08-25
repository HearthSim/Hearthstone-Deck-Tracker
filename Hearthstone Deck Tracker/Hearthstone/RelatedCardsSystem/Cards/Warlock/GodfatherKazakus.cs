using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class GodfatherKazakus : ICardWithRelatedCards
{
	private static readonly string[] TrialOptions =
	{
		HearthDb.CardIds.NonCollectible.Warlock.GodfatherKazakus_TonicOfTyrannyToken,
		HearthDb.CardIds.NonCollectible.Warlock.GodfatherKazakus_ConvictedForConspiracyToken,
		HearthDb.CardIds.NonCollectible.Warlock.GodfatherKazakus_SentencedForSmugglingToken,
		HearthDb.CardIds.NonCollectible.Warlock.GodfatherKazakus_CrateOfContrabandToken,
		HearthDb.CardIds.NonCollectible.Warlock.GodfatherKazakus_SpuriousShivToken,
		HearthDb.CardIds.NonCollectible.Warlock.GodfatherKazakus_CriminalContractToken,
		HearthDb.CardIds.NonCollectible.Warlock.GodfatherKazakus_PotionOfPerjuryToken,
		HearthDb.CardIds.NonCollectible.Warlock.GodfatherKazakus_SwillOfSuggestibilityToken,
		HearthDb.CardIds.NonCollectible.Warlock.GodfatherKazakus_DetainedForDestructionToken,
	};

	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.GodfatherKazakus;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		TrialOptions.Select(Database.GetCardFromId).ToList();
}
