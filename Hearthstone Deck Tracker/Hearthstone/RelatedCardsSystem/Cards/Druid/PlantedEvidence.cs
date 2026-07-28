namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Discover a spell. It costs (2) less this turn."
public class PlantedEvidence : NatureStudies
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.PlantedEvidence;
}

public class PlantedEvidenceCore : PlantedEvidence
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.PlantedEvidenceCorePlaceholder;
}
