#region

using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class SecretHelper
	{

		public SecretHelper(HeroClass heroClass, int id, int turnPlayed)
		{
			Id = id;
            TurnPlayed = turnPlayed;
			HeroClass = heroClass;
			PossibleSecrets = new Dictionary<string, bool>();

			foreach (var cardId in GetSecretIds(heroClass))
			{
				PossibleSecrets[cardId] = true;
			}
		}

		public int Id { get; private set; }
		public int TurnPlayed { get; private set; }
		public HeroClass HeroClass { get; private set; }
		public Dictionary<string, bool> PossibleSecrets { get; set; }


		public static int GetMaxSecretCount(HeroClass heroClass)
		{
			return GetSecretIds(heroClass).Count;
		}

		public static List<string> GetSecretIds(HeroClass heroClass)
		{
			switch(heroClass)
			{
				case HeroClass.Hunter:
					return CardIds.Secrets.Hunter.All;
				case HeroClass.Mage:
					return CardIds.Secrets.Mage.All;
				case HeroClass.Paladin:
					return CardIds.Secrets.Paladin.All;
				default:
					return new List<string>();
			}
		}
	}
}