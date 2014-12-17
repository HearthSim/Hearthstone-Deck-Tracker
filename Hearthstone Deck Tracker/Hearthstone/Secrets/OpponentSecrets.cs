using System.Collections.Generic;
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

		public void NewSecretPlayed(int id)
		{
			Secrets.Add(new SecretHelper(HeroClass, id));
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
				foreach(var secret in Secrets)
					secret.PossibleSecrets[index] = false;
			else
				foreach(var secret in Secrets)
					secret.PossibleSecrets[index] = true;
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
			var count = SecretHelper.GetMaxSecretCount(HeroClass);
			var returnThis = new Secret[count];
			for(int i = 0; i < count; i++)
				returnThis[i] = new Secret(SecretHelper.GetSecretIds(HeroClass)[i], 1);
			return returnThis;
		}

	}
}
