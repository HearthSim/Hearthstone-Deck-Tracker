using System.Collections.Generic;
using System.Linq;
using BobsBuddy;
using BobsBuddy.Factory;
using BobsBuddy.HeroPowers;
using BobsBuddy.Minions.Beast;
using BobsBuddy.Minions.Mech;
using BobsBuddy.Minions.Pirate;
using BobsBuddy.Minions.Undead;
using BobsBuddy.Simulation;
using BobsBuddy.Spells;
using BobsBuddy.Trinkets;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using static HearthDb.CardIds;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.BobsBuddy
{
	internal static class BobsBuddyUtils
	{
		private const string ReplicatingMenace_Normal = NonCollectible.Neutral.ReplicatingMenace_ReplicatingMenaceEnchantmentBATTLEGROUNDS;
		private const string ReplicatingMenace_Golden = NonCollectible.Neutral.ReplicatingMenace_ReplicatingMenaceEnchantmentTavernBrawl;
		private const string LivingSpores = NonCollectible.Neutral.LivingSporesToken2;
		public const string RebornRiteEnchmantment = NonCollectible.Neutral.RebornRites_RebornRiteEnchantmentTavernBrawl;
		public const string SneedsEnchantment = NonCollectible.Neutral.Sneed_Replicate;
		internal const string RebornRite = NonCollectible.Neutral.RebornRitesTavernBrawl;

		internal static Minion GetMinionFromEntity(Simulator sim, bool player, Entity entity, IEnumerable<Entity> attachedEntities)
		{
			var cardId = entity.Info.LatestCardId ?? "Unknown";
			var minion = sim.MinionFactory.CreateFromCardId(cardId, player);

			minion.PrimaryRace = (Race)entity.GetTag(GameTag.CARDRACE);
			minion.baseAttack = entity.GetTag(GameTag.ATK);
			minion.baseHealth = entity.GetTag(GameTag.HEALTH);
			minion.taunt = entity.HasTag(GameTag.TAUNT);
			minion.div = entity.HasTag(GameTag.DIVINE_SHIELD);
			minion.cleave = MinionFactory.cardIDsWithCleave.Contains(minion.CardID);
			minion.poisonous = entity.HasTag(GameTag.POISONOUS);
			minion.venomous = entity.HasTag(GameTag.VENOMOUS);
			minion.windfury = entity.HasTag(GameTag.WINDFURY);
			minion.megaWindfury = entity.HasTag(GameTag.MEGA_WINDFURY) || MinionFactory.cardIdsWithMegaWindfury.Contains(cardId);
			minion.stealth = entity.HasTag(GameTag.STEALTH);
			minion.golden = entity.HasTag(GameTag.PREMIUM);
			minion.tier = entity.GetTag(GameTag.TECH_LEVEL);
			minion.reborn = entity.HasTag(GameTag.REBORN);
			minion.ScriptDataNum1 = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			minion.ScriptDataNum2 = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);

			var dbfId = entity.Card.DbfId;
			var m1 = entity.GetTag(GameTag.MODULAR_ENTITY_PART_1);
			var m2 = entity.GetTag(GameTag.MODULAR_ENTITY_PART_2);
			if(m1 > 0 && m2 > 0 && (m1 == dbfId || m2 == dbfId))
			{
				var modularCard = Database.GetCardFromDbfId(m1 == dbfId ? m2 : m1, false);
				if(modularCard != null)
					minion.AttachModularEntity(modularCard.Id);
			}

			//Vanilla health
			if(minion.golden && MinionFactory.cardIdsWithoutPremiumImplementations.Contains(cardId))
			{
				minion.vanillaAttack *= 2;
				minion.vanillaHealth *= 2;
			}

			foreach(var attached in attachedEntities)
			{
				switch(attached.CardId)
				{
					case ReplicatingMenace_Normal:
						minion.AdditionalDeathrattles.Add(ReplicatingMenace.Deathrattle(false));
						break;
					case ReplicatingMenace_Golden:
						minion.AdditionalDeathrattles.Add(ReplicatingMenace.Deathrattle(true));
						break;
					case LivingSpores:
						minion.AdditionalDeathrattles.Add(GenericDeathrattles.Plants);
						break;
					case SneedsEnchantment:
						minion.AdditionalDeathrattles.Add(GenericDeathrattles.SneedHeroPower);
						break;
					case NonCollectible.Neutral.SurfnSurf_CrabRidingEnchantment:
						minion.AdditionalDeathrattles.Add(GenericDeathrattles.SurfNSurfSpellDeathrattle);
						break;
					case NonCollectible.Neutral.SurfnSurf_CrabRiding:
						minion.AdditionalDeathrattles.Add(GenericDeathrattles.SurfNSurfSpellDeathrattleGolden);
						break;
					case NonCollectible.Neutral.Brukan_ElementEarth:
						minion.AdditionalDeathrattles.Add(GenericDeathrattles.EarthInvocationDeathrattle);
						break;
					case NonCollectible.Neutral.Brukan_EarthRecollection:
						minion.AdditionalDeathrattles.Add(BrukanInvocationDeathrattles.Earth);
						break;
					case NonCollectible.Neutral.Brukan_FireRecollection:
						minion.AdditionalDeathrattles.Add(BrukanInvocationDeathrattles.Fire);
						break;
					case NonCollectible.Neutral.Brukan_WaterRecollection:
						minion.AdditionalDeathrattles.Add(BrukanInvocationDeathrattles.Water);
						break;
					case NonCollectible.Neutral.Brukan_LightningRecollection:
						minion.AdditionalDeathrattles.Add(BrukanInvocationDeathrattles.Lightning);
						break;
					case NonCollectible.Neutral.Wingmen_WingmenEnchantmentTavernBrawl:
						minion.HasWingmen = true;
						break;
					case NonCollectible.Neutral.RecurringNightmare_NightmareInsideEnchantment:
						minion.AdditionalDeathrattles.Add(RecurringNightmare.SummonDeathrattle(false));
						break;
					case NonCollectible.Neutral.RecurringNightmare_NightmareInside:
						minion.AdditionalDeathrattles.Add(RecurringNightmare.SummonDeathrattle(true));
						break;
					case NonCollectible.Neutral.SkyPirateFlagbearer_FlagbearingEnchantment:
						minion.AdditionalDeathrattles.Add(Scallywag.Deathrattle(false));
						break;
					case NonCollectible.Neutral.SkyPirateFlagbearer_Flagbearing:
						minion.AdditionalDeathrattles.Add(Scallywag.Deathrattle(true));
						break;
					case NonCollectible.Neutral.Leapfrogger_LeapfrogginEnchantment:
						minion.AdditionalDeathrattles.Add(Leapfrogger.Deathrattle(false));
						break;
					case NonCollectible.Neutral.Leapfrogger_Leapfroggin:
						minion.AdditionalDeathrattles.Add(Leapfrogger.Deathrattle(true));
						break;
					case NonCollectible.Neutral.RustyTrident_TridentsTreasureEnchantment:
						minion.AdditionalDeathrattles.Add(RustyTrident.Deathrattle());
						break;
					case NonCollectible.Neutral.HoggyBank_GemInTheBankEnchantment:
						minion.AdditionalDeathrattles.Add(HoggyBank.Deathrattle());
						break;
					case NonCollectible.Neutral.JarredFrostling_FrostyGlobeEnchantment:
						minion.AdditionalDeathrattles.Add(JarredFrostling.Deathrattle());
						break;
					case NonCollectible.Neutral.BloodGem2:
						var atk = attached.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
						var health = attached.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
						minion.SetBloodGemStats(atk, health);
						break;
					default:
						if(attached.Card.TypeEnum == CardType.ENCHANTMENT && attached.CardId != null)
						{
							var enchantment = sim.EnchantmentFactory.Create(attached.CardId, minion.ControlledByPlayer);
							if(enchantment != null)
							{
								enchantment.ScriptDataNum1 = attached.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
								enchantment.ScriptDataNum2 = attached.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
								minion.AttachEnchantment(enchantment);
							}
						}
						break;
				}
			}

			minion.game_id = entity.Id;

			return minion;
		}

		internal static Objective GetObjectiveFromEntity(ObjectiveFactory factory, bool player, Entity entity)
		{
			var objective = factory.Create(entity.CardId ?? "", player);
			var scriptDataNum = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			if(scriptDataNum > 0)
				objective.ScriptDataNum1 = scriptDataNum;
			return objective;
		}

		internal static bool WasHeroPowerActivated(Entity? heroPower)
			=> heroPower != null && (heroPower.HasTag(GameTag.EXHAUSTED) || heroPower.HasTag(GameTag.BACON_HERO_POWER_ACTIVATED));

		internal static IOrderedEnumerable<Entity> GetOrderedMinions(IEnumerable<Entity> board)
			=> board.Where(x => x.IsMinion).Select(x => x.Clone()).OrderBy(x => x.GetTag(GameTag.ZONE_POSITION));

		private static string? _versionString;
		internal static string VersionString => _versionString ??= "v" + typeof(SimulationRunner).Assembly.GetName().Version.ToVersionString();
	}
}
