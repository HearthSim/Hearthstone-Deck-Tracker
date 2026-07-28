using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Summon a random minion with Cost equal to your weapon's Attack."
public class Steeldancer : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Steeldancer;

	protected override int? TargetCost(Player player, Entity? hoveredEntity)
	{
		if(!player.IsLocalPlayer)
			return null;
		return player.Board.FirstOrDefault(e => e.IsWeapon)?.Attack;
	}
}
