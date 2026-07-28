using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Summon a random 6-Cost minion, {1} times." — the base cost of 6 is raised by Leyline
// effect increases (Mystic Runesaber, The Arcanomicon), and the number of summons is
// 1 + the extra triggers from LeylineExtraTriggerCounter (Surge Needle, The Arcanomicon).
// Each summon is an independent draw from the same cost bucket, so the target is yielded once per summon.
public class CrystallizedLeyline : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.CrystallizedLeyline;

	protected override int? TargetCost(Player player, Entity? hoveredEntity) =>
		6 + (GetCounter<LeylineEffectIncreaseCounter>(player)?.Value ?? 0);

	protected override IEnumerable<(int Cost, int Offset)> GetTargets(Player player, Entity? hoveredEntity)
	{
		if(TargetCost(player, hoveredEntity) is int cost)
		{
			var summons = 1 + (GetCounter<LeylineExtraTriggerCounter>(player)?.Value ?? 0);
			for(var i = 0; i < summons; i++)
				yield return (cost, 0);
		}
	}
}
