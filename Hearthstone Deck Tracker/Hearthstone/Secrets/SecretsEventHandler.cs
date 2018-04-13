using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using static Hearthstone_Deck_Tracker.Hearthstone.CardIds.Secrets;

namespace Hearthstone_Deck_Tracker.Hearthstone.Secrets
{
	public abstract class SecretsEventHandler
	{
		private const int AvengeDelay = 50;
		private int _avengeDeathRattleCount;
		private bool _awaitingAvenge;
		private int _lastCompetitiveSpiritCheck;
		private HashSet<Entity> EntititesInHandOnMinionsPlayed = new HashSet<Entity>();

		private bool FreeSpaceOnBoard => Game.OpponentMinionCount < 7;
		private bool FreeSpaceInHand => Game.OpponentHandCount < 10;
		private bool HandleAction => HasActiveSecrets && Config.Instance.AutoGrayoutSecrets;
		private bool IsAnyMinionInOpponentsHand => EntititesInHandOnMinionsPlayed.Any(entity => entity.IsMinion);

		public List<Secret> Secrets { get; } = new List<Secret>();

		protected abstract IGame Game { get; }
		protected abstract bool HasActiveSecrets { get; }
		public abstract bool Exclude(string cardId, bool invokeCallback = true);
		public abstract void Exclude(List<string> cardIds);

		public virtual void Reset()
		{
			_avengeDeathRattleCount = 0;
			_awaitingAvenge = false;
			_lastCompetitiveSpiritCheck = 0;
			EntititesInHandOnMinionsPlayed.Clear();
		}

		public void HandleAttack(Entity attacker, Entity defender, bool fastOnly = false)
		{
			if(!HandleAction)
				return;

			if(attacker.GetTag(GameTag.CONTROLLER) == defender.GetTag(GameTag.CONTROLLER))
				return;

			var exclude = new List<string>();

			var freeSpaceOnBoard = FreeSpaceOnBoard;
			if(freeSpaceOnBoard)
				exclude.Add(Paladin.NobleSacrifice);

			if(defender.IsHero)
			{
				if(!fastOnly)
				{
					if(freeSpaceOnBoard)
						exclude.Add(Hunter.BearTrap);
					exclude.Add(Mage.IceBarrier);
				}

				if(freeSpaceOnBoard)
					exclude.Add(Hunter.WanderingMonster);

				exclude.Add(Hunter.ExplosiveTrap);

				if(Game.IsMinionInPlay)
					exclude.Add(Hunter.Misdirection);

				if(attacker.IsMinion && Game.PlayerMinionCount > 1)
					exclude.Add(Rogue.SuddenBetrayal);

				if(attacker.IsMinion)
				{
					exclude.Add(Mage.Vaporize);
					exclude.Add(Hunter.FreezingTrap);
				}
			}
			else
			{
				if(!fastOnly && freeSpaceOnBoard)
				{
					exclude.Add(Hunter.SnakeTrap);
					exclude.Add(Hunter.VenomstrikeTrap);
				}

				if(attacker.IsMinion)
					exclude.Add(Hunter.FreezingTrap);
			}
			Exclude(exclude);
		}

		/// <summary>
		/// see http://hearthstone.gamepedia.com/Advanced_rulebook#Combat for fast vs. slow secrets
		/// a few fast secrets can modify combat
		/// freezing trap and vaporize remove the attacking minion
		/// misdirection, noble sacrifice change the target
		/// if multiple secrets are in play and a fast secret triggers,
		/// we need to eliminate older secrets which would have been triggered by the attempted combat
		/// </summary>
		public void HandleFastCombat(Entity entity)
		{
			if(!HandleAction)
				return;
			if(!entity.HasCardId || Game.ProposedAttacker == 0 || Game.ProposedDefender == 0)
				return;
			if(!FastCombat.Contains(entity.CardId))
				return;
			if(Game.Entities.TryGetValue(Game.ProposedAttacker, out var attacker)
				&& Game.Entities.TryGetValue(Game.ProposedDefender, out var defender))
				HandleAttack(attacker, defender, true);
		}

		public void HandleMinionPlayed()
		{
			if(!HandleAction)
				return;

			var exclude = new List<string>();

			exclude.Add(Hunter.Snipe);
			exclude.Add(Mage.ExplosiveRunes);
			exclude.Add(Mage.PotionOfPolymorph);
			exclude.Add(Paladin.Repentance);

			if(FreeSpaceOnBoard)
				exclude.Add(Mage.MirrorEntity);

			if(FreeSpaceInHand)
				exclude.Add(Mage.FrozenClone);

			//Hidden cache will only trigger if the opponent has a minion in hand. 
			//We might not know this for certain - requires additional tracking logic.
			var cardsInOpponentsHand = Game.Entities.Select(kvp => kvp.Value).Where(e => e.IsInHand && e.IsControlledBy(Game.Opponent.Id)).ToList();
			foreach (var cardInOpponentsHand in cardsInOpponentsHand)
				EntititesInHandOnMinionsPlayed.Add(cardInOpponentsHand);

			if (IsAnyMinionInOpponentsHand)
				exclude.Add(Hunter.HiddenCache);

			Exclude(exclude);
		}

		public void HandleMinionDeath(Entity entity)
		{
			if(!HandleAction)
				return;

			var exclude = new List<string>();

			if(FreeSpaceInHand)
			{
				exclude.Add(Mage.Duplicate);
				exclude.Add(Paladin.GetawayKodo);
				exclude.Add(Rogue.CheatDeath);
			}

			if(Game.OpponentEntity.GetTag(GameTag.NUM_FRIENDLY_MINIONS_THAT_DIED_THIS_TURN) >= 1)
				exclude.Add(Paladin.HandOfSalvation);

			var numDeathrattleMinions = 0;
			if(entity.IsActiveDeathrattle)
			{
				if(!CardIds.DeathrattleSummonCardIds.TryGetValue(entity.CardId ?? "", out numDeathrattleMinions))
				{
					if(entity.CardId == HearthDb.CardIds.Collectible.Neutral.Stalagg
						&& Game.Opponent.Graveyard.Any(x => x.CardId == HearthDb.CardIds.Collectible.Neutral.Feugen)
						|| entity.CardId == HearthDb.CardIds.Collectible.Neutral.Feugen
						&& Game.Opponent.Graveyard.Any(x => x.CardId == HearthDb.CardIds.Collectible.Neutral.Stalagg))
						numDeathrattleMinions = 1;
				}
				if(Game.Entities.Any(x => x.Value.CardId == HearthDb.CardIds.NonCollectible.Druid.SouloftheForest_SoulOfTheForestEnchantment
										&& x.Value.GetTag(GameTag.ATTACHED) == entity.Id))
					numDeathrattleMinions++;
				if(Game.Entities.Any(x => x.Value.CardId == HearthDb.CardIds.NonCollectible.Shaman.AncestralSpirit_AncestralSpiritEnchantment
										&& x.Value.GetTag(GameTag.ATTACHED) == entity.Id))
					numDeathrattleMinions++;
			}

			if(Game.OpponentEntity != null && Game.OpponentEntity.HasTag(GameTag.EXTRA_DEATHRATTLES))
				numDeathrattleMinions *= Game.OpponentEntity.GetTag(GameTag.EXTRA_DEATHRATTLES) + 1;

			HandleAvengeAsync(numDeathrattleMinions);

			// redemption never triggers if a deathrattle effect fills up the board
			// effigy can trigger ahead of the deathrattle effect, but only if effigy was played before the deathrattle minion
			if(Game.OpponentMinionCount < 7 - numDeathrattleMinions)
				exclude.Add(Paladin.Redemption);

			// TODO: break ties when Effigy + Deathrattle played on the same turn
			exclude.Add(Mage.Effigy);

			Exclude(exclude);
		}

		public async void HandleAvengeAsync(int deathRattleCount)
		{
			if(!HandleAction)
				return;
			_avengeDeathRattleCount += deathRattleCount;
			if(_awaitingAvenge)
				return;
			_awaitingAvenge = true;
			if(Game.OpponentMinionCount != 0)
			{
				await Game.GameTime.WaitForDuration(AvengeDelay);
				if(Game.OpponentMinionCount - _avengeDeathRattleCount > 0)
					Exclude(Paladin.Avenge);
			}
			_awaitingAvenge = false;
			_avengeDeathRattleCount = 0;
		}

		public void HandleOpponentDamage(Entity entity)
		{
			if(!HandleAction)
				return;
			if(entity.IsHero && entity.IsControlledBy(Game.Opponent.Id))
			{
				Exclude(Paladin.EyeForAnEye);
				Exclude(Rogue.Evasion);
			}
		}

		public void HandleTurnsInPlayChange(Entity entity, int turn)
		{
			if(!HandleAction)
				return;
			if(turn <= _lastCompetitiveSpiritCheck || !entity.IsMinion
				|| !entity.IsControlledBy(Game.Opponent.Id) || !Game.OpponentEntity.IsCurrentPlayer)
				return;
			_lastCompetitiveSpiritCheck = turn;
			Exclude(Paladin.CompetitiveSpirit);
		}

		public async void HandleCardPlayed(Entity entity)
		{
			if(!HandleAction)
				return;

			var exclude = new List<string>();

			if(FreeSpaceOnBoard)
			{
				if(Game.PlayerEntity?.GetTag(GameTag.NUM_CARDS_PLAYED_THIS_TURN) >= 3)
					exclude.Add(Hunter.RatTrap);
			}

			if(FreeSpaceInHand)
			{
				if(Game.PlayerEntity?.GetTag(GameTag.NUM_CARDS_PLAYED_THIS_TURN) >= 3)
					exclude.Add(Paladin.HiddenWisdom);
			}

			if(entity.IsSpell)
			{
				exclude.Add(Mage.Counterspell);

				if(Game.OpponentHandCount < 10)
					exclude.Add(Mage.ManaBind);

				if(Game.OpponentMinionCount < 7)
				{
					//CARD_TARGET is set after ZONE, wait for 50ms gametime before checking
					await Game.GameTime.WaitForDuration(50);
					if(entity.HasTag(GameTag.CARD_TARGET)
						&& Game.Entities.TryGetValue(entity.GetTag(GameTag.CARD_TARGET), out Entity target) && target.IsMinion)
						exclude.Add(Mage.Spellbender);
					exclude.Add(Hunter.CatTrick);
				}
			}
			else if(entity.IsMinion && Game.PlayerMinionCount > 3)
				exclude.Add(Paladin.SacredTrial);

			Exclude(exclude);
		}

		public void HandleHeroPower()
		{
			if(!HandleAction)
				return;
			Exclude(Hunter.DartTrap);
		}

		public void OnEntityRevealedAsMinion(Entity entity)
		{
			if (EntititesInHandOnMinionsPlayed.Contains(entity) && entity.IsMinion)
				Exclude(Hunter.HiddenCache);
		}

		public void OnNewSecret(Secret secret)
		{
			if (secret.Entity.IsClass(CardClass.HUNTER))
				EntititesInHandOnMinionsPlayed.Clear();
		}
	}
}
