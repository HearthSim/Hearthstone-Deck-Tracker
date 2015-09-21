#region

using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class SecretHelper
	{

		public SecretHelper(HeroClass heroClass, int id, bool stolen)
		{
			Id = id;
			Stolen = stolen;
			HeroClass = heroClass;
			PossibleSecrets = new Dictionary<string, bool>();

			foreach (var cardId in GetSecretIds(heroClass))
			{
				PossibleSecrets[cardId] = true;
			}
		}

		public int Id { get; private set; }
		public bool Stolen { get; private set; }
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
					return CardIds.SecretIdsHunter;
				case HeroClass.Mage:
					return CardIds.SecretIdsMage;
				case HeroClass.Paladin:
					return CardIds.SecretIdsPaladin;
				default:
					return new List<string>();
			}
		}
	}
}