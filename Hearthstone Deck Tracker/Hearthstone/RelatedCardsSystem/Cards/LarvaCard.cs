using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards;

// Shared base for cards that give Larvae: "1-Cost Larva with a Combo that triggers a Hero Power from
// another class." One Larva per class; the player's own class is excluded. The Neutral Teal Larva
// ("No Spoilers!") is left out until its class is known.
public abstract class LarvaCard : ICardWithRelatedCards
{
	private static readonly string[] Larvae =
	{
		HearthDb.CardIds.NonCollectible.Priest.BroodchamberTunneler_PaleLarvaToken,
		HearthDb.CardIds.NonCollectible.Shaman.BroodchamberTunneler_BlueLarvaToken,
		HearthDb.CardIds.NonCollectible.Rogue.BroodchamberTunneler_BlackLarvaToken,
		HearthDb.CardIds.NonCollectible.Paladin.BroodchamberTunneler_YellowLarvaToken,
		HearthDb.CardIds.NonCollectible.Hunter.BroodchamberTunneler_GreenLarvaToken,
		HearthDb.CardIds.NonCollectible.Druid.BroodchamberTunneler_BrownLarvaToken,
		HearthDb.CardIds.NonCollectible.Warlock.BroodchamberTunneler_PurpleLarvaToken,
		HearthDb.CardIds.NonCollectible.Mage.BroodchamberTunneler_CyanLarvaToken,
		HearthDb.CardIds.NonCollectible.Warrior.BroodchamberTunneler_RedLarvaToken,
		HearthDb.CardIds.NonCollectible.Demonhunter.BroodchamberTunneler_NeonLarvaToken,
		HearthDb.CardIds.NonCollectible.Deathknight.BroodchamberTunneler_DarkLarvaToken,
	};

	public abstract string GetCardId();

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		Larvae
			.Select(Database.GetCardFromId)
			.Where(card => card != null && !card.IsClass(player.CurrentClass))
			.ToList();
}
