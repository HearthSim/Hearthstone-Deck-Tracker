using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Get two random spells from spell schools you haven't cast this game."
// Same unplayed-school pool as DiscoveryOfMagic; two random draws instead of a Discover.
public class SirenSong : DiscoveryOfMagic
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.SirenSong;
	protected override PickConfig Config => new PickConfig(1, 2, true);
}
