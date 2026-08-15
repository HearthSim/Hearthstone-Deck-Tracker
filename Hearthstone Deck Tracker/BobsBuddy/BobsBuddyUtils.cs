using System;
using System.Collections.Generic;
using System.Linq;
using BobsBuddy;
using BobsBuddy.Enchantments;
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
		private const string WhirringProtector_Normal = NonCollectible.Neutral.WhirringProtector_WhirringProtectorEnchantment;
		private const string WhirringProtector_Golden = NonCollectible.Neutral.WhirringProtector_WhirringProtector2;
		private const string LivingSpores = NonCollectible.Neutral.LivingSporesToken2;
		public const string RebornRiteEnchmantment = NonCollectible.Neutral.RebornRites_RebornRiteEnchantmentTavernBrawl;
		public const string SneedsEnchantment = NonCollectible.Neutral.Sneed_Replicate;
		internal const string RebornRite = NonCollectible.Neutral.RebornRitesTavernBrawl;

		internal static Minion GetMinionFromEntity(Simulator sim, bool player, Entity entity, IEnumerable<Entity> attachedEntities, IReadOnlyDictionary<int, Entity>? allEntities = null)
		{
			var cardId = entity.Info.LatestCardId ?? "Unknown";
			var minion = sim.MinionFactory.CreateFromCardId(cardId, player);

			minion.PrimaryRace = (Race)entity.GetTag(GameTag.CARDRACE);
			minion.baseAttack = entity.GetTag(GameTag.ATK);
			minion.baseHealth = entity.GetTag(GameTag.HEALTH) - entity.GetTag(GameTag.DAMAGE);
			minion.maxAttack = entity.GetTag(GameTag.ATK);
			minion.maxHealth = entity.GetTag(GameTag.HEALTH);
			minion.taunt = entity.HasTag(GameTag.TAUNT);
			minion.div = entity.HasTag(GameTag.DIVINE_SHIELD) ? 1 : 0;
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
			minion.ScriptDataNum3 = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_3);
			minion.ScriptDataNum4 = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_4);

			// LatestCard, not Card: MODULAR_ENTITY_PART tags hold the dbf id of what the entity
			// currently is, which differs from Card for in-place transforms via CHANGE_ENTITY
			var dbfId = entity.LatestCard.DbfId;
			var m1 = entity.GetTag(GameTag.MODULAR_ENTITY_PART_1);
			var m2 = entity.GetTag(GameTag.MODULAR_ENTITY_PART_2);
			if(m1 > 0 && m2 > 0 && (m1 == dbfId || m2 == dbfId))
			{
				var modularCard = Database.GetCardFromDbfId(m1 == dbfId ? m2 : m1, false);
				if(modularCard != null)
				{
					var modularMinion = sim.MinionFactory.CreateFromCardId(modularCard.Id, player);
					minion.AttachedModularEntity = modularMinion;
					modularMinion.AttachedTo = minion;
				}
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
					case WhirringProtector_Normal:
						minion.AdditionalRallies.Add(WhirringProtector.Rally(false));
						break;
					case WhirringProtector_Golden:
						minion.AdditionalRallies.Add(WhirringProtector.Rally(true));
						break;
					case LivingSpores:
						minion.AdditionalDeathrattles.Add(GenericDeathrattles.Plants);
						break;
					case SneedsEnchantment:
						minion.AdditionalDeathrattles.Add(GenericDeathrattles.SneedHeroPower);
						break;
					case NonCollectible.Neutral.Brukan_ElementEarth:
						minion.AdditionalDeathrattles.Add(GenericDeathrattles.EarthInvocationDeathrattle);
						break;
					case NonCollectible.Neutral.SurfnSurf_CrabRidingEnchantment:
						minion.AdditionalDeathrattles.Add(GenericDeathrattles.Crab);
						break;
					case NonCollectible.Neutral.SurfnSurf_CrabRiding:
						minion.AdditionalDeathrattles.Add(GenericDeathrattles.CrabGolden);
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
					case NonCollectible.Neutral.BloodGems:
						var atk = attached.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
						var health = attached.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
						minion.SetBloodGemStats(atk, health);
						break;
					default:
						if(attached.LatestCard.TypeEnum == CardType.ENCHANTMENT && attached.Info.LatestCardId != null)
						{
							var enchantment = sim.EnchantmentFactory.Create(attached.Info.LatestCardId, minion.ControlledByPlayer);
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

			if(attachedEntities.Any(e => e.HasTag(GameTag.MAGNETIC)))
			{
				if(minion.IsMech())
					CheckForMagnetizedDeathrattles(minion, entity, attachedEntities, allEntities);
				// Not just mech here, because Technical Element can magnetize to Elementals
				CheckForSurfnSurfFromMagnetizedModules(minion, entity, allEntities);
			}

			minion.game_id = entity.Id;

			return minion;
		}

		// Magnetized deathrattles (e.g., Auto Assembler) can be *hiding* if initially attached
		// to a magnetic minion that was then tripled and magnetized to another mech.
		private static void CheckForMagnetizedDeathrattles(Minion minion, Entity host, IEnumerable<Entity> attachedEntities, IReadOnlyDictionary<int, Entity>? allEntities)
		{
			// Required to resolve the chain of magnetized
			if(allEntities == null)
				return;

			// Specific handling for: Auto Assembler
			// Each attached enchantment's CREATOR is the magnetic card that produced it; take each distinct id once.
			// Exclude the HOST: an enchantment the host created on itself is already turned into an
			// AutoAssemblerEnchantment by the attachedEntities above (only magnetized modules are hidden).
			foreach(var magneticId in attachedEntities.Select(e => e.GetTag(GameTag.CREATOR)).Where(id => id > 0 && id != host.Id).Distinct())
			{
				if(!allEntities.TryGetValue(magneticId, out var magnetic))
					continue;

				var enchantCount = allEntities.Values.Count(x => x.IsAttachedTo(magneticId) && x.CardId == AutoAssemblerEnchantment.CardId);
				var goldenEnchantCount = allEntities.Values.Count(x => x.IsAttachedTo(magneticId) && x.CardId == AutoAssemblerEnchantmentGolden.CardId);

				for(var i = 0; i < enchantCount; i++)
					minion.AdditionalDeathrattles.Add(AutoAssembler.Deathrattle());
				for(var i = 0; i < goldenEnchantCount; i++)
					minion.AdditionalDeathrattles.Add(AutoAssembler.GoldenDeathrattle());
			}

			// Future magnetic deathrattles can be added/handled here.
		}

		// A magnetized module keeps its own attached enchantments, including Surf n' Surf Spellcraft spells
		// cast on a Technical Element, which is then magnetized to another host minion.
		// The magnetized module records its host in TAG_SCRIPT_DATA_NUM_1 when it magnetizes.
		private static void CheckForSurfnSurfFromMagnetizedModules(Minion minion, Entity host, IReadOnlyDictionary<int, Entity>? allEntities)
		{
			if(allEntities == null)
				return;

			var magnetizedModules = allEntities.Values
				.Where(x => x.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1) == host.Id
					&& x.HasTag(GameTag.MAGNETIC)
					&& x.LatestCard.TypeEnum == CardType.MINION)
				.OrderBy(x => x.Id);

			foreach(var module in magnetizedModules)
			{
				var granted = allEntities.Values
					.Where(x => x.IsAttachedTo(module.Id))
					.OrderBy(x => x.Id);
				foreach(var attached in granted)
				{
					switch(attached.CardId)
					{
						case NonCollectible.Neutral.SurfnSurf_CrabRidingEnchantment:
							minion.AdditionalDeathrattles.Add(GenericDeathrattles.Crab);
							break;
						case NonCollectible.Neutral.SurfnSurf_CrabRiding:
							minion.AdditionalDeathrattles.Add(GenericDeathrattles.CrabGolden);
							break;
					}
				}
			}
		}

		private static void SetScriptDataProperties(dynamic item, Entity entity)
		{
			var scriptDataNum1 = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			var scriptDataNum2 = entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2);
			if(scriptDataNum1 > 0)
				item.ScriptDataNum1 = scriptDataNum1;
			if(scriptDataNum2 > 0)
				item.ScriptDataNum2 = scriptDataNum2;
		}

		internal static Objective GetObjectiveFromEntity(ObjectiveFactory factory, bool player, Entity entity)
		{
			var objective = factory.Create(entity.CardId ?? "", player);
			SetScriptDataProperties(objective, entity);
			return objective;
		}

		internal static Trinket GetTrinketFromEntity(TrinketFactory factory, bool player, Entity entity)
		{
			// Use LatestCardId, not CardId. A Lesser/Greater Crystal Ball that has transformed into a
			// copy of a trinket keeps its stale token CardId (BACON trinket entities are not updated by
			// CHANGE_ENTITY), while LatestCardId always reflects the copied card.
			var cardId = entity.Info.LatestCardId ?? entity.CardId ?? "";
			var trinket = factory.Create(cardId, player);
			SetScriptDataProperties(trinket, entity);

			// Set slot that holds the trinket (i.e., Fantastic Treasure, Growing Collection,
			// Lesser Trinket, Greater Trinket); necessary to order friendly start of combat triggers
			var containerCardId = entity.Info.CardIdBeforeReveal ?? entity.CardId;
			if(containerCardId != null && containerCardId != cardId)
				trinket.ContainerCardId = containerCardId;

			// Special handling for replica cathedral
			if(cardId == NonCollectible.Neutral.ReplicaCathedral)
				trinket.ScriptDataNum1 = entity.GetTag((GameTag)4696);

			trinket.game_id = entity.Id;

			return trinket;
		}

		internal static bool IsOpponentHeroKelThuzad()
		{
			var heroEntityId = Core.Game.OpponentEntity?.GetTag(GameTag.HERO_ENTITY);
			var heroEntity = Core.Game.Opponent.PlayerEntities?.FirstOrDefault(x => x.Id == heroEntityId);
			return heroEntity?.CardId == NonCollectible.Neutral.KelthuzadTavernBrawl2;
		}

		// NUM_RESOURCES_SPENT_THIS_GAME is never set for the opponent, but can be inferred from Malorne
		internal static int GetResourcesSpentThisGameFromMalorne(Entity malorne, IEnumerable<Entity> attachedEntities)
		{
			var enchantments = attachedEntities
				.Where(x => x.CardId != NonCollectible.Neutral.ForestLordCenarius_PowerOfAncients)
				.ToList();
			var attackBuffs = enchantments.Sum(x => x.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1));
			var healthBuffs = enchantments.Sum(x => x.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2));
			// GetTag(HEALTH) is the max health, unaffected by damage. Taking the min of the two increases accuracy
			// in case there is a buff that never appears as an enchantment attached to the minion.
			var aura = Math.Min(
				malorne.GetTag(GameTag.ATK) - malorne.LatestCard.Attack - attackBuffs,
				malorne.GetTag(GameTag.HEALTH) - malorne.LatestCard.Health - healthBuffs);
			if(malorne.CardId == NonCollectible.Neutral.ForestLordCenarius_Malorne2)
				aura /= 2;
			return aura > 0 ? aura * 3 : 0;
		}

		internal static bool WasHeroPowerActivated(Entity? heroPower)
			=> heroPower != null && (heroPower.HasTag(GameTag.EXHAUSTED) || heroPower.HasTag(GameTag.BACON_HERO_POWER_ACTIVATED));

		internal static IOrderedEnumerable<Entity> GetOrderedMinions(IEnumerable<Entity> board)
			=> board.Where(x => x.IsMinion).Select(x => x.Clone()).OrderBy(x => x.GetTag(GameTag.ZONE_POSITION));

		internal static IOrderedEnumerable<Entity> GetOrderedHandEntities(IEnumerable<Entity> hand)
			=> hand.OrderBy(x => x.GetTag(GameTag.ZONE_POSITION));

		private static string? _versionString;
		internal static string VersionString => _versionString ??= "v" + typeof(SimulationRunner).Assembly.GetName().Version.ToVersionString();
	}
}
