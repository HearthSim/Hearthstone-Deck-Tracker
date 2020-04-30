using System.Collections.Generic;
using System.Linq;
using BobsBuddy;
using BobsBuddy.Simulation;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static HearthDb.CardIds;

namespace Hearthstone_Deck_Tracker.BobsBuddy
{
	internal static class BobsBuddyUtils
	{
		private const string ReplicatingMenace = NonCollectible.Neutral.ReplicatingMenace_ReplicatingMenaceEnchantment;
		private const string ReplicatingMenace_Golden = NonCollectible.Neutral.ReplicatingMenace_ReplicatingMenaceEnchantmentTavernBrawl;
		private const string LivingSpores = NonCollectible.Neutral.LivingSporesToken2;
		private const string RagePotion = NonCollectible.Neutral.RagePotion_RagePotionEnchantmentTavernBrawl;
		private const string RebornRite = NonCollectible.Neutral.RebornRites_RebornRiteEnchantmentTavernBrawl;

		internal static Minion GetMinionFromEntity(Entity entity, IEnumerable<Entity> attachedEntities) 
		{
			var minion = Minion.FromHearthDbCardID(entity.Info.LatestCardId);

			minion.baseAttack = entity.GetTag(GameTag.ATK);
			minion.baseHealth = entity.GetTag(GameTag.HEALTH);
			minion.taunt = entity.HasTag(GameTag.TAUNT);
			minion.div = entity.HasTag(GameTag.DIVINE_SHIELD);
			minion.cleave = Minion.cardIDsWithCleave.Contains(minion.cardID);
			minion.poisonous = entity.HasTag(GameTag.POISONOUS);
			minion.windfury = entity.HasTag(GameTag.WINDFURY);
			minion.megaWindfury = entity.HasTag(GameTag.MEGA_WINDFURY);
			minion.golden = entity.HasTag(GameTag.PREMIUM);
			minion.tier = entity.GetTag(GameTag.TECH_LEVEL);
			minion.reborn = entity.HasTag(GameTag.REBORN);

			// Attached Deathrattles
			minion.mechDeathCount = attachedEntities.Count(x => x.CardId == ReplicatingMenace);
			minion.mechDeathCountGold = attachedEntities.Count(x => x.CardId == ReplicatingMenace_Golden);
			minion.plantDeathCount = attachedEntities.Count(x => x.CardId == LivingSpores);

			// Putricide hero power
			if(attachedEntities.Any(x => x.CardId == RagePotion))
				minion.baseAttack += 10;

			// Lich King hero power
			if(attachedEntities.Any(x => x.CardId == RebornRite))
				minion.reborn = true;

			return minion;
		}

		internal static bool HeroPowerUsed(Entity heroPower)
			=> heroPower != null && (heroPower.HasTag(GameTag.EXHAUSTED) || heroPower.HasTag(GameTag.PENDING_TRIGGER));

		internal static IOrderedEnumerable<Entity> GetOrderedMinions(IEnumerable<Entity> board)
			=> board.Where(x => x.IsMinion).Select(x => x.Clone()).OrderBy(x => x.GetTag(GameTag.ZONE_POSITION));

		internal static void LogMinionStats(Minion m)
			=> Log.Debug($"{m.minionName} basestats (with static effects): ({m.baseAttack}, {m.baseHealth}), " +
				$"double on effects ({m.attack()}, {m.health()})");

		internal static void LogMinionDeathrattles(Minion m)
			=> Log.Debug($"{m.minionName}: mechDeathCount={m.mechDeathCount}," +
				$"mechDeathCountGold={m.mechDeathCountGold}, " +
				$"plantDeathCount={m.plantDeathCount} ");

		private static string _versionString;
		internal static string VersionString => _versionString ?? (_versionString = "v" + typeof(SimulationRunner).Assembly.GetName().Version.ToVersionString());
	}
}
