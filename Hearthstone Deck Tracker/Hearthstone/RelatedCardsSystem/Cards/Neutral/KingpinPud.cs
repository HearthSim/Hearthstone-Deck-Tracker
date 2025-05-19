using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class KingpinPud: ResurrectionCard
{

	private readonly List<string> _ogreGangs = new() {
		HearthDb.CardIds.Collectible.Neutral.OgreGangOutlaw,
		HearthDb.CardIds.Collectible.Neutral.OgreGangRider,
		HearthDb.CardIds.Collectible.Neutral.OgreGangAce,
		HearthDb.CardIds.Collectible.Neutral.BoulderfistOgreCore,
		HearthDb.CardIds.Collectible.Neutral.BoulderfistOgreLegacy,
		HearthDb.CardIds.Collectible.Neutral.BoulderfistOgreVanilla,
	};

	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.KingpinPud;

	protected override bool FilterCard(Card card) => _ogreGangs.Contains(card.Id);

	protected override bool ResurrectsMultipleCards() => true;
}
