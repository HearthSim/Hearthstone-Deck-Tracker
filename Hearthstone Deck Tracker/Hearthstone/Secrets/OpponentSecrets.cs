#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

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
        public int proposedAttackerEntityId { get;  set; }
        public int proposedDefenderEntityId { get; set; }
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
				{
					helper.PossibleSecrets[cardId] = cardId == knownCardId;
				}
			}
			Secrets.Add(helper);
			Logger.WriteLine("Added secret with id:" + id, "OpponentSecrets");
		}

        public void SecretRemoved(int id, string cardId)
        {
            int index = Secrets.FindIndex(s => s.Id == id);
	        if(index == -1)
			{
				Logger.WriteLine(string.Format("Secret with id={0}, cardId={1} not found when trying to remove it.", id, cardId), "OpponentSecrets");
				return;
	        }
            Entity attacker, defender;
            Game.Entities.TryGetValue(proposedAttackerEntityId, out attacker);
            Game.Entities.TryGetValue(proposedDefenderEntityId, out defender);

            // see http://hearthstone.gamepedia.com/Advanced_rulebook#Combat for fast vs. slow secrets

            // a few fast secrets can modify combat
            // freezing trap and vaporize remove the attacking minion
            // misdirection, noble sacrifice change the target

            // if multiple secrets are in play and a fast secret triggers,
            // we need to eliminate older secrets which would have been triggered by the attempted combat
            if (CardIds.Secrets.FastCombat.Contains(cardId) && attacker != null && defender != null)
            {
                ZeroFromAttack(Game.Entities[proposedAttackerEntityId], Game.Entities[proposedDefenderEntityId], true, index);
            }

            Secrets.Remove(Secrets[index]);
            Logger.WriteLine("Removed secret with id:" + id, "OpponentSecrets");
        }

        public void ZeroFromAttack(Entity attacker, Entity defender, bool fastOnly = false, int stopIndex = -1)
        {
            if (!Config.Instance.AutoGrayoutSecrets)
                return;

            if (stopIndex == -1)
                stopIndex = Secrets.Count;

            SetZeroOlder(CardIds.Secrets.Paladin.NobleSacrifice, stopIndex);

            if (defender.IsHero)
            {
                if (!fastOnly)
                {
                    SetZeroOlder(CardIds.Secrets.Hunter.BearTrap, stopIndex);
                    SetZeroOlder(CardIds.Secrets.Mage.IceBarrier, stopIndex);
                }

                SetZeroOlder(CardIds.Secrets.Hunter.ExplosiveTrap, stopIndex);

                if (Game.IsMinionInPlay)
                    SetZeroOlder(CardIds.Secrets.Hunter.Misdirection, stopIndex);

                if (attacker.IsMinion)
                {
                    SetZeroOlder(CardIds.Secrets.Mage.Vaporize, stopIndex);
                    SetZeroOlder(CardIds.Secrets.Hunter.FreezingTrap, stopIndex);
                }
            }
            else
            {
                if (!fastOnly)
                    SetZeroOlder(CardIds.Secrets.Hunter.SnakeTrap, stopIndex);

                if (attacker.IsMinion)
                    SetZeroOlder(CardIds.Secrets.Hunter.FreezingTrap, stopIndex);
            }

            if (Helper.MainWindow != null)
                Helper.MainWindow.Overlay.ShowSecrets();
        }

        public void ClearSecrets()
		{
			Secrets.Clear();
			Logger.WriteLine("Cleared secrets", "OpponentSecrets");
		}

		public void SetMax(string cardId)
		{
			foreach(var secret in Secrets)
				secret.PossibleSecrets[cardId] = true;
		}

        public void SetZero(string cardId)
        {
            SetZeroOlder(cardId, Secrets.Count);
        }

        public void SetZeroOlder(string cardId, int stopIndex)
        {
            for (int index = 0; index < stopIndex; index++)
                Secrets[index].PossibleSecrets[cardId] = false;
        }

        public List<Secret> GetSecrets()
		{
			var returnThis = DisplayedClasses.SelectMany(SecretHelper.GetSecretIds).Select(cardId => new Secret(cardId, 0)).ToList();

			foreach (var secret in Secrets)
			{
				foreach (var possible in secret.PossibleSecrets)
				{
					if (possible.Value)
					{
						returnThis.Find(x => x.CardId == possible.Key).Count = 1;
					}
				}

			}

			return returnThis;
		}

		public List<Secret> GetDefaultSecrets(HeroClass heroClass)
		{
			var count = SecretHelper.GetMaxSecretCount(heroClass);
			var returnThis = new List<Secret>();

			foreach(var cardId in SecretHelper.GetSecretIds(heroClass))
				returnThis.Add(new Secret(cardId, 1));

			return returnThis;
		}
	}
}