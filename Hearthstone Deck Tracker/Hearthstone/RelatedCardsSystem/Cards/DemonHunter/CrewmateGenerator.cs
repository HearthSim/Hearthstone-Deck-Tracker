using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public abstract class CrewmateGenerator
{
	private readonly List<Card?> _crewmates = new List<Card?> {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.VoroneiRecruiter_AdminCrewmateToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.VoroneiRecruiter_EngineCrewmateToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.VoroneiRecruiter_HelmCrewmateToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.VoroneiRecruiter_GunnerCrewmateToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.VoroneiRecruiter_MedicalCrewmateToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.VoroneiRecruiter_ReconCrewmateToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.VoroneiRecruiter_ResearchCrewmateToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.VoroneiRecruiter_TacticalCrewmateToken),
	};

	public List<Card?> GetRelatedCards(Player player) =>
		_crewmates;
}
