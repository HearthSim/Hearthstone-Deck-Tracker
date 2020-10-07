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
		private const string RebornRiteEnchmantment = NonCollectible.Neutral.RebornRites_RebornRiteEnchantmentTavernBrawl;
		internal const string RebornRite = NonCollectible.Neutral.RebornRitesTavernBrawl;

		internal static Minion GetMinionFromEntity(Entity entity, IEnumerable<Entity> attachedEntities) 
		{
			var minion = MinionFactory.GetMinionFromCardid(entity.Info.LatestCardId);

			minion.baseAttack = entity.GetTag(GameTag.ATK);
			minion.baseHealth = entity.GetTag(GameTag.HEALTH);
			minion.taunt = entity.HasTag(GameTag.TAUNT);
			minion.div = entity.HasTag(GameTag.DIVINE_SHIELD);
			minion.cleave = MinionFactory.cardIDsWithCleave.Contains(minion.cardID);
			minion.poisonous = entity.HasTag(GameTag.POISONOUS);
			minion.windfury = entity.HasTag(GameTag.WINDFURY);
			minion.megaWindfury = entity.HasTag(GameTag.MEGA_WINDFURY) || MinionFactory.cardIdsWithMegaWindfury.Contains(entity.CardId);
			minion.golden = entity.HasTag(GameTag.PREMIUM);
			minion.tier = entity.GetTag(GameTag.TECH_LEVEL);
			minion.reborn = entity.HasTag(GameTag.REBORN);

			//Vanilla health
			if(minion.golden && MinionFactory.cardIdsWithoutPremiumImplementations.Contains(entity.Info.LatestCardId))
				minion.vanillaHealth *= 2;

			// Attached Deathrattles
			minion.mechDeathCount = attachedEntities.Count(x => x.CardId == ReplicatingMenace);
			minion.mechDeathCountGold = attachedEntities.Count(x => x.CardId == ReplicatingMenace_Golden);
			minion.plantDeathCount = attachedEntities.Count(x => x.CardId == LivingSpores);

			// Lich King hero power
			if(attachedEntities.Any(x => x.CardId == RebornRiteEnchmantment))
				minion.receivesLichKingPower = true;

			minion.game_id = entity.Id;

			Log.Info($"Added {entity.Name}, ({minion.baseAttack}, {minion.baseHealth}, controller {entity.GetTag(GameTag.CONTROLLER)}, creator {entity.Info.GetCreatorId()}.");

			return minion;
		}

		internal static bool HeroPowerUsed(Entity heroPower)
			=> heroPower != null && (heroPower.HasTag(GameTag.EXHAUSTED) || heroPower.HasTag(GameTag.BACON_HERO_POWER_ACTIVATED));

		internal static IOrderedEnumerable<Entity> GetOrderedMinions(IEnumerable<Entity> board)
			=> board.Where(x => x.IsMinion).Select(x => x.Clone()).OrderBy(x => x.GetTag(GameTag.ZONE_POSITION));

		private static string _versionString;
		internal static string VersionString => _versionString ?? (_versionString = "v" + typeof(SimulationRunner).Assembly.GetName().Version.ToVersionString());
	}
}
