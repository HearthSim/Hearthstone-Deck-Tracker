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
		private const int MultiSecretResolveDelay = 750;
		private int _avengeDeathRattleCount;
		private bool _awaitingAvenge;
		private int _lastStartOfTurnCheck;
		private int _lastStartOfTurnDamageCheck;
		private int _lastStartOfTurnMinionCheck;
		private HashSet<Entity> EntititesInHandOnMinionsPlayed = new HashSet<Entity>();

		private int _lastPlayedMinionId;
		protected List<MultiIdCard> SavedSecrets = new List<MultiIdCard>();

		private bool FreeSpaceOnBoard => Game.OpponentBoardCount < 7;
		private bool FreeSpaceInHand => Game.OpponentHandCount < 10;
		private bool HandleAction => HasActiveSecrets && Config.Instance.AutoGrayoutSecrets;
		private bool IsAnyMinionInOpponentsHand => EntititesInHandOnMinionsPlayed.Any(entity => entity.IsMinion);

		public List<Secret> Secrets { get; } = new List<Secret>();

		public List<int> OpponentTookDamageDuringTurns = new List<int>();

		public Dictionary<int, Dictionary<int, int>> EntityDamageDealtHistory = new Dictionary<int, Dictionary<int, int>>();

		private List<Entity> _triggeredSecrets = new List<Entity>();

		protected abstract IGame Game { get; }
		protected abstract bool HasActiveSecrets { get; }
		public abstract bool Exclude(MultiIdCard cardId, bool invokeCallback = true);
		public abstract void Exclude(List<MultiIdCard> cardIds);
		public abstract void Refresh();

		public virtual void Reset()
		{
			_avengeDeathRattleCount = 0;
			_awaitingAvenge = false;
			_lastStartOfTurnCheck = 0;
			_lastStartOfTurnDamageCheck = 0;
			_lastStartOfTurnMinionCheck = 0;
			OpponentTookDamageDuringTurns.Clear();
			EntititesInHandOnMinionsPlayed.Clear();
		}

		public void HandleAttack(Entity attacker, Entity defender, bool fastOnly = false)
		{
			if(!HandleAction)
				return;

			if(attacker.GetTag(GameTag.CONTROLLER) == defender.GetTag(GameTag.CONTROLLER))
				return;

			var exclude = new List<MultiIdCard>();

			var freeSpaceOnBoard = FreeSpaceOnBoard;
			if(freeSpaceOnBoard)
				exclude.Add(Paladin.NobleSacrifice);

			if(attacker.IsHero)
			{

			}
			else
			{
				exclude.Add(Paladin.JudgmentOfJustice);
			}

			if(defender.IsHero)
			{
				if(!fastOnly && attacker.Health >= 1)
				{
					if(freeSpaceOnBoard)
						exclude.Add(Hunter.BearTrap);

					if(Game.Entities.Values.Any(x =>
													x.IsInPlay &&
													(x.IsHero || x.IsMinion) &&
													!x.HasTag(GameTag.IMMUNE) &&
													x != attacker &&
													x != defender))
						exclude.Add(Hunter.Misdirection);

					if(attacker.IsMinion)
					{
						if(Game.PlayerMinionCount > 1)
							exclude.Add(Rogue.SuddenBetrayal);

						exclude.Add(Mage.FlameWard);
						exclude.Add(Hunter.FreezingTrap);
						exclude.Add(Mage.Vaporize);
						if(FreeSpaceOnBoard)
							exclude.Add(Rogue.ShadowClone);
					}
				}

				if(freeSpaceOnBoard)
				{
					exclude.Add(Hunter.WanderingMonster);

					if(attacker.IsMinion)
						exclude.Add(Mage.VengefulVisage);
				}

				exclude.Add(Mage.IceBarrier);
				exclude.Add(Hunter.ExplosiveTrap);
			}
			else
			{
				exclude.Add(Rogue.Bamboozle);
				if (!defender.HasTag(GameTag.DIVINE_SHIELD))
					exclude.Add(Paladin.AutodefenseMatrix);

				if(freeSpaceOnBoard)
				{
					exclude.Add(Mage.SplittingImage);
					exclude.Add(Hunter.PackTactics);
					exclude.Add(Hunter.SnakeTrap);
					exclude.Add(Hunter.VenomstrikeTrap);
					//I think most of the secrets here could (and maybe should) check for this, but this one definitley does because of Hysteria.
					if(Game.PlayerEntity.IsCurrentPlayer)
						exclude.Add(Mage.OasisAlly);
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
			if(!FastCombat.Contains(entity.CardId!))
				return;
			if(Game.Entities.TryGetValue(Game.ProposedAttacker, out var attacker)
				&& Game.Entities.TryGetValue(Game.ProposedDefender, out var defender))
				HandleAttack(attacker, defender, true);
		}

		public void HandleMinionPlayed(Entity entity)
		{
			if(!HandleAction)
				return;

			_lastPlayedMinionId = entity.Id;

			var exclude = new List<MultiIdCard>();

			if(!entity.HasTag(GameTag.DORMANT))
			{
				SaveSecret(Hunter.Snipe);
				exclude.Add(Hunter.Snipe);
				SaveSecret(Mage.ExplosiveRunes);
				exclude.Add(Mage.ExplosiveRunes);
				SaveSecret(Mage.Objection);
				exclude.Add(Mage.Objection);
				SaveSecret(Mage.PotionOfPolymorph);
				exclude.Add(Mage.PotionOfPolymorph);
				SaveSecret(Paladin.Repentance);
				exclude.Add(Paladin.Repentance);
			}

			if(FreeSpaceOnBoard)
			{
				SaveSecret(Mage.MirrorEntity);
				exclude.Add(Mage.MirrorEntity);
				SaveSecret(Rogue.Ambush);
				exclude.Add(Rogue.Ambush);
			}

			if(FreeSpaceInHand)
				exclude.Add(Mage.FrozenClone);

			exclude.Add(Rogue.Kidnap);

			//Hidden cache will only trigger if the opponent has a minion in hand. 
			//We might not know this for certain - requires additional tracking logic.
			var cardsInOpponentsHand = Game.Entities.Select(kvp => kvp.Value).Where(e => e.IsInHand && e.IsControlledBy(Game.Opponent.Id)).ToList();
			foreach (var cardInOpponentsHand in cardsInOpponentsHand)
				EntititesInHandOnMinionsPlayed.Add(cardInOpponentsHand);

			if (IsAnyMinionInOpponentsHand)
				exclude.Add(Hunter.HiddenCache);

			Exclude(exclude);
		}

		public void HandleOpponentMinionDeath(Entity entity)
		{
			if(!HandleAction)
				return;

			var exclude = new List<MultiIdCard>();

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
			if(Game.OpponentBoardCount < 7 - numDeathrattleMinions)
				exclude.Add(Paladin.Redemption);

			// TODO: break ties when Effigy + Deathrattle played on the same turn
			exclude.Add(Mage.Effigy);
			exclude.Add(Hunter.EmergencyManeuvers);

			Exclude(exclude);
		}

		public void HandlePlayerMinionDeath(Entity entity)
		{
			if(entity.Id == _lastPlayedMinionId && SavedSecrets.Count > 0)
			{
				foreach(var savedSecret in SavedSecrets)
					foreach(var secret in Secrets)
						secret.Include(savedSecret);
				Refresh();
			}
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

		public void OnNewBlock()
		{
			EntityDamageDealtHistory.Clear();
		}

		public async void HandleEntityDamageAsync(Entity dealer, Entity target, int damage)
		{
			if(target != null)
			{
				if(target.IsHero && target.IsControlledBy(Game.Opponent.Id))
				{
					if(!target.HasTag(GameTag.IMMUNE))
					{
						Exclude(Paladin.EyeForAnEye);
						Exclude(Rogue.Evasion);
						OpponentTookDamageDuringTurns.Add(Game.GetTurnNumber());
					}
				}
				if(dealer != null)
				{
					if(dealer.IsMinion && dealer.IsControlledBy(Game.Player.Id))
					{
						if(!EntityDamageDealtHistory.TryGetValue(dealer.Id, out var history))
						{
							EntityDamageDealtHistory[dealer.Id] = new Dictionary<int, int>();
						}
						if(!EntityDamageDealtHistory[dealer.Id].TryGetValue(target.Id, out var targetHistory))
						{
							EntityDamageDealtHistory[dealer.Id][target.Id] = 0;
						}
						EntityDamageDealtHistory[dealer.Id][target.Id] += damage;
						var damageDealt = EntityDamageDealtHistory[dealer.Id][target.Id];
						await Game.GameTime.WaitForDuration(100);

						//We check both heaolth and zone because sometimes after the await the dealer's health will revert to that of the original card.
						if(damageDealt >= 3 && dealer.Health > 0 && (Zone)dealer.GetTag(GameTag.ZONE) != Zone.GRAVEYARD)
							Exclude(Paladin.Reckoning);
					}
				}
			}
		}

		public void HandleEntityLostArmor(Entity entity, int value)
		{
			if(value <= 0)
				return;
			if(entity != null)
			{
				if(entity.IsHero && entity.IsControlledBy(Game.Opponent.Id))
				{
					if(!entity.HasTag(GameTag.IMMUNE))
					{
						OpponentTookDamageDuringTurns.Add(Game.GetTurnNumber());
					}
				}
			}
		}

		public void HandleTurnsInPlayChange(Entity entity, int turn)
		{
			if(!HandleAction)
				return;
			if(Game.OpponentEntity.IsCurrentPlayer && turn > _lastStartOfTurnCheck)
			{
				_lastStartOfTurnCheck = turn;
				Exclude(Rogue.Perjury);
			}
			if(Game.OpponentEntity.IsCurrentPlayer && turn > _lastStartOfTurnMinionCheck)
			{
				if(entity.IsMinion && entity.IsControlledBy(Game.Opponent.Id))
				{
					_lastStartOfTurnMinionCheck = turn;
					Exclude(Paladin.CompetitiveSpirit);
					if(Game.OpponentMinionCount >= 2 && FreeSpaceOnBoard)
						Exclude(Hunter.OpenTheCages);
				}
			}
			if(Game.OpponentEntity.IsCurrentPlayer && turn > _lastStartOfTurnDamageCheck)
			{
				_lastStartOfTurnDamageCheck = turn;
				var turnToCheck = turn - (Game.PlayerEntity?.HasTag(GameTag.FIRST_PLAYER) ?? false ? 0 : 1);
				if(!OpponentTookDamageDuringTurns.Contains(turnToCheck))
					Exclude(Mage.RiggedFaireGame);
			}
		}

		public void SecretTriggered(Entity secret) => _triggeredSecrets.Add(secret);

		public async void HandleCardPlayed(Entity entity, string parentBlockCardId)
		{
			if(!HandleAction)
				return;

			SavedSecrets.Clear();

			var exclude = new List<MultiIdCard>();


			if(Game.PlayerEntity?.GetTag(GameTag.NUM_CARDS_PLAYED_THIS_TURN) >= 3)
			{
				exclude.Add(Hunter.MotionDenied);

				if(FreeSpaceOnBoard)
				{
					exclude.Add(Hunter.RatTrap);
					exclude.Add(Paladin.GallopingSavior);
				}

				if(FreeSpaceInHand)
					exclude.Add(Paladin.HiddenWisdom);
			}

			if(entity.IsSpell)
			{
				if(parentBlockCardId == HearthDb.CardIds.Collectible.Rogue.SparkjoyCheat)
					return;

				_triggeredSecrets.Clear();
				if(Game.OpponentSecretCount > 1)
					await Game.GameTime.WaitForDuration(MultiSecretResolveDelay);

				// Counterspell/Ice trap order may matter in rare edge cases where both are in play.
				// This is currently not handled.

				exclude.Add(Mage.Counterspell);
				if(_triggeredSecrets.FirstOrDefault(x => x.CardId != null && Mage.Counterspell == x.CardId) != null)
				{
					Exclude(new List<MultiIdCard> { Mage.Counterspell });
					return;
				}

				exclude.Add(Hunter.IceTrap);
				if(_triggeredSecrets.FirstOrDefault(x => x.CardId != null && Hunter.IceTrap == x.CardId) != null)
				{
					Exclude(new List<MultiIdCard> { Hunter.IceTrap });
					return;
				}

				exclude.Add(Paladin.OhMyYogg);

				if(Game.OpponentMinionCount > 0)
					exclude.Add(Paladin.NeverSurrender);

				if(Game.OpponentHandCount < 10)
				{
					exclude.Add(Rogue.DirtyTricks);
					exclude.Add(Mage.ManaBind);
				}

				if(FreeSpaceOnBoard)
				{
					//CARD_TARGET is set after ZONE, wait for 50ms gametime before checking
					await Game.GameTime.WaitForDuration(50);
					if(entity.HasTag(GameTag.CARD_TARGET)
						&& Game.Entities.TryGetValue(entity.GetTag(GameTag.CARD_TARGET), out Entity target) && target.IsMinion)
						exclude.Add(Mage.Spellbender);
					exclude.Add(Hunter.CatTrick);
					exclude.Add(Mage.NetherwindPortal);
					exclude.Add(Rogue.StickySituation);
				}

				if (Game.PlayerMinionCount > 0)
					exclude.Add(Hunter.PressurePlate);
			}
			else if(entity.IsMinion && Game.PlayerMinionCount > 3)
				exclude.Add(Paladin.SacredTrial);

			Exclude(exclude);
		}

		public void HandleCardDrawn(Entity entity)
		{
			if(!HandleAction)
				return;

			var exclude = new List<MultiIdCard>();

			//Check against 1 because the tag hasn't been incremented by hs by the time this is getting called
			if(Game.PlayerEntity?.GetTag(GameTag.NUM_CARDS_DRAWN_THIS_TURN) >= 1)
				exclude.Add(Rogue.Shenanigans);

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

		public void SaveSecret(MultiIdCard secret)
		{
			if(!Secrets.Any(s => s.IsExcluded(secret)))
				SavedSecrets.Add(secret);
		}

		public void HandlePlayerTurnStart()
		{
			SavedSecrets.Clear();
		}

		public void HandleOpponentTurnStart()
		{
			// This triggers regardless of cards in hand
			if (Game.Player.CardsPlayedThisTurn.Count > 0)
				Exclude(Rogue.Plagiarize);
		}

		public void HandleManaRemaining(int mana)
		{
			if(mana == 0 && FreeSpaceInHand)
				Exclude(Rogue.DoubleCross);
		}
	}
}
