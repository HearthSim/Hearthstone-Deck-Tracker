using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker
{
	public class OpponentSecrets
	{
		public List<SecretHelper> Secrets { get; private set; }
		public HeroClass HeroClass { get; set; }

		public OpponentSecrets()
		{
			Secrets = new List<SecretHelper>();
		}

		public void NewSecretPlayed(int id, bool stolen)
		{
			Secrets.Add(new SecretHelper(HeroClass, id, stolen));
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
			var index = SecretHelper.GetSecretIndex(HeroClass, cardId);
			if(index == -1)
				return;
			if(Secrets.Any(s => s.PossibleSecrets[index]))
				SetZero(index);
			else
				SetMax(index);
		}

		public void SetMax(string cardId)
		{
			var index = SecretHelper.GetSecretIndex(HeroClass, cardId);
			if(index != -1)
				SetMax(index);
        }

		public void SetMax(int index)
		{
			foreach(var secret in Secrets)
				if(index > 0 || index < secret.PossibleSecrets.Length)
					secret.PossibleSecrets[index] = true;
		}

		public void SetZero(string cardId)
		{
			var index = SecretHelper.GetSecretIndex(HeroClass, cardId);
			if(index != -1)
				SetZero(index);
        }

		public void SetZero(int index)
		{
			foreach(var secret in Secrets)
				if(index > 0 || index < secret.PossibleSecrets.Length)
					secret.PossibleSecrets[index] = false;
		}

		public Secret[] GetSecrets()
		{
			var count = SecretHelper.GetMaxSecretCount(HeroClass);
            var returnThis = new Secret[count];
			for(int i = 0; i < count; i++)
				returnThis[i] = new Secret(SecretHelper.GetSecretIds(HeroClass)[i], 0);
			foreach(var secret in Secrets)
				for(int i = 0; i < count; i++)
					if(secret.PossibleSecrets[i])
						returnThis[i].Count++;
			return returnThis;
		}

		public Secret[] GetDefaultSecrets(HeroClass heroClass)
		{
			var count = SecretHelper.GetMaxSecretCount(heroClass);
			var returnThis = new Secret[count];
			for(int i = 0; i < count; i++)
				returnThis[i] = new Secret(SecretHelper.GetSecretIds(heroClass)[i], 1);
			return returnThis;
		}

	}
}
