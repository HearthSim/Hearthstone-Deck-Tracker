#region

using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class OpponentSecrets
	{
		public OpponentSecrets(GameV2 game)
		{
			Secrets = new List<SecretHelper>();
			Game = game;
		}

		public List<SecretHelper> Secrets { get; private set; }
		public int ProposedAttackerEntityId { get; set; }
		public int ProposedDefenderEntityId { get; set; }
		public GameV2 Game { get; private set; }

		public List<HeroClass> DisplayedClasses
		{
			get { return Secrets.Select(x => x.HeroClass).Distinct().OrderBy(x => x).ToList(); }
		}

		public int GetIndexOffset(HeroClass heroClass)
		{
			switch(heroClass)
			{
				case HeroClass.Hunter:
					return 0;
				case HeroClass.Mage:
					if(DisplayedClasses.Contains(HeroClass.Hunter))
						return SecretHelper.GetMaxSecretCount(HeroClass.Hunter);
					return 0;
				case HeroClass.Paladin:
					if(DisplayedClasses.Contains(HeroClass.Hunter) && DisplayedClasses.Contains(HeroClass.Mage))
						return SecretHelper.GetMaxSecretCount(HeroClass.Hunter) + SecretHelper.GetMaxSecretCount(HeroClass.Mage);
					if(DisplayedClasses.Contains(HeroClass.Hunter))
						return SecretHelper.GetMaxSecretCount(HeroClass.Hunter);
					if(DisplayedClasses.Contains(HeroClass.Mage))
						return SecretHelper.GetMaxSecretCount(HeroClass.Mage);
					return 0;
			}
			return 0;
		}

		public HeroClass? GetHeroClass(string cardId)
		{
			HeroClass heroClass;
			if(!Enum.TryParse(Database.GetCardFromId(cardId).PlayerClass, out heroClass))
				return null;
			return heroClass;
		}

		public void Trigger(string cardId)
		{
			if(Secrets.Any(s => s.PossibleSecrets[cardId]))
				SetZero(cardId);
			else
				SetMax(cardId);
		}

		public void NewSecretPlayed(HeroClass heroClass, int id, int turn, string knownCardId = null)
		{
			var helper = new SecretHelper(heroClass, id, turn);
			if(knownCardId != null)
			{
				foreach(var cardId in SecretHelper.GetSecretIds(heroClass))
					helper.PossibleSecrets[cardId] = cardId == knownCardId;
			}
			Secrets.Add(helper);
			Log.Info("Added secret with id:" + id);
		}

		public void SecretRemoved(int id, string cardId)
		{
			int index = Secrets.FindIndex(s => s.Id == id);
			if(index == -1)
			{
				Log.Warn($"Secret with id={id}, cardId={cardId} not found when trying to remove it.");
				return;
			}
			Entity attacker, defender;
			Game.Entities.TryGetValue(ProposedAttackerEntityId, out attacker);
			Game.Entities.TryGetValue(ProposedDefenderEntityId, out defender);

			// see http://hearthstone.gamepedia.com/Advanced_rulebook#Combat for fast vs. slow secrets

			// a few fast secrets can modify combat
			// freezing trap and vaporize remove the attacking minion
			// misdirection, noble sacrifice change the target

			// if multiple secrets are in play and a fast secret triggers,
			// we need to eliminate older secrets which would have been triggered by the attempted combat
			if(CardIds.Secrets.FastCombat.Contains(cardId) && attacker != null && defender != null)
				ZeroFromAttack(Game.Entities[ProposedAttackerEntityId], Game.Entities[ProposedDefenderEntityId], true, index);

			Secrets.Remove(Secrets[index]);
			Log.Info("Removed secret with id:" + id);
		}

		public void ZeroFromAttack(Entity attacker, Entity defender, bool fastOnly = false, int stopIndex = -1)
		{
			if(!Config.Instance.AutoGrayoutSecrets)
				return;

			if(stopIndex == -1)
				stopIndex = Secrets.Count;

			if(Game.OpponentMinionCount < 7)
				SetZeroOlder(CardIds.Secrets.Paladin.NobleSacrifice, stopIndex);

			if(defender.IsHero)
			{
				if(!fastOnly)
				{
					if(Game.OpponentMinionCount < 7)
						SetZeroOlder(CardIds.Secrets.Hunter.BearTrap, stopIndex);
					SetZeroOlder(CardIds.Secrets.Mage.IceBarrier, stopIndex);
				}

				SetZeroOlder(CardIds.Secrets.Hunter.ExplosiveTrap, stopIndex);

				if(Game.IsMinionInPlay)
					SetZeroOlder(CardIds.Secrets.Hunter.Misdirection, stopIndex);

				if(attacker.IsMinion)
				{
					SetZeroOlder(CardIds.Secrets.Mage.Vaporize, stopIndex);
					SetZeroOlder(CardIds.Secrets.Hunter.FreezingTrap, stopIndex);
				}
			}
			else
			{
				if(!fastOnly && Game.OpponentMinionCount < 7)
					SetZeroOlder(CardIds.Secrets.Hunter.SnakeTrap, stopIndex);

				if(attacker.IsMinion)
					SetZeroOlder(CardIds.Secrets.Hunter.FreezingTrap, stopIndex);
			}

			if(Core.MainWindow != null)
				Core.Overlay.ShowSecrets();
		}

		public void ClearSecrets()
		{
			Secrets.Clear();
			Log.Info("Cleared secrets");
		}

		public void SetMax(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			foreach(var secret in Secrets)
				secret.PossibleSecrets[cardId] = true;
		}

		public void SetZero(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			SetZeroOlder(cardId, Secrets.Count);
		}

		public void SetZeroOlder(string cardId, int stopIndex)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			for(var index = 0; index < stopIndex; index++)
				Secrets[index].PossibleSecrets[cardId] = false;
			if(stopIndex > 0)
				Log.Info("Set secret to zero: " + Database.GetCardFromId(cardId));
		}

		public List<Secret> GetSecrets()
		{
			var returnThis = DisplayedClasses.SelectMany(SecretHelper.GetSecretIds).Select(cardId => new Secret(cardId, 0)).ToList();

			foreach(var secret in Secrets)
			{
				foreach(var possible in secret.PossibleSecrets)
				{
					if(possible.Value)
					{
						var s = returnThis.FirstOrDefault(x => x.CardId == possible.Key);
						if(s != null)
							s.Count++;
					}
				}
			}

			return returnThis;
		}

		public List<Secret> GetDefaultSecrets(HeroClass heroClass)
		{
			return SecretHelper.GetSecretIds(heroClass).Select(cardId => new Secret(cardId, 1)).ToList();
		}
	}
}