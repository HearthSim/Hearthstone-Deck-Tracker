#region

using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class OpponentSecrets
	{
		public OpponentSecrets()
		{
			Secrets = new List<SecretHelper>();
		}

		public List<SecretHelper> Secrets { get; private set; }

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

		public void NewSecretPlayed(HeroClass heroClass, int id, bool stolen)
		{
			Secrets.Add(new SecretHelper(heroClass, id, stolen));
			Logger.WriteLine("Added secret with id:" + id, "OpponentSecrets");
		}

		public void SecretRemoved(int id)
		{
			var secret = Secrets.FirstOrDefault(s => s.Id == id);
			Secrets.Remove(secret);
			Logger.WriteLine("Removed secret with id:" + id, "OpponentSecrets");
		}

		public void ClearSecrets()
		{
			Secrets.Clear();
			Logger.WriteLine("Cleared secrets", "OpponentSecrets");
		}

		public void Trigger(string cardId)
		{
			var heroClass = GetHeroClass(cardId);
			if(!heroClass.HasValue)
				return;
			var index = SecretHelper.GetSecretIndex(heroClass.Value, cardId);
			if(index == -1)
				return;
			//index += GetIndexOffset(heroClass.Value);
			if(Secrets.Where(s => s.HeroClass == heroClass).Any(s => s.PossibleSecrets[index]))
				SetZero(index, heroClass.Value);
			else
				SetMax(index, heroClass.Value);
		}

		public void SetMax(string cardId, HeroClass? heroClass)
		{
			if(heroClass == null)
			{
				heroClass = GetHeroClass(cardId);
				if(!heroClass.HasValue)
					return;
			}
			var index = SecretHelper.GetSecretIndex(heroClass.Value, cardId);
			if(index != -1)
				SetMax(index, heroClass.Value);
		}

		public void SetMax(int index, HeroClass heroClass)
		{
			foreach(var secret in Secrets.Where(s => s.HeroClass == heroClass))
			{
				if(index > 0 || index < secret.PossibleSecrets.Length)
					secret.PossibleSecrets[index] = true;
			}
		}

		public void SetZero(string cardId, HeroClass? heroClass)
		{
			if(heroClass == null)
			{
				heroClass = GetHeroClass(cardId);
				if(!heroClass.HasValue)
					return;
			}
			var index = SecretHelper.GetSecretIndex(heroClass.Value, cardId);
			if(index != -1)
				SetZero(index, heroClass.Value);
		}

		public void SetZero(int index, HeroClass heroClass)
		{
			foreach(var secret in Secrets.Where(s => s.HeroClass == heroClass))
			{
				if(index > 0 || index < secret.PossibleSecrets.Length)
					secret.PossibleSecrets[index] = false;
			}
		}

		public Secret[] GetSecrets()
		{
			var secrets = DisplayedClasses.SelectMany(SecretHelper.GetSecretIds).Select(cardId => new Secret(cardId, 0)).ToArray();
			foreach(var secret in Secrets)
			{
				var offset = GetIndexOffset(secret.HeroClass);
				for(var i = 0; i < secret.PossibleSecrets.Count(); i++)
				{
					if(secret.PossibleSecrets[i])
						secrets[i+offset].Count++;
				}
			}
			return secrets;
		}

		public Secret[] GetDefaultSecrets(HeroClass heroClass)
		{
			var count = SecretHelper.GetMaxSecretCount(heroClass);
			var returnThis = new Secret[count];
			for(var i = 0; i < count; i++)
				returnThis[i] = new Secret(SecretHelper.GetSecretIds(heroClass)[i], 1);
			return returnThis;
		}
	}
}