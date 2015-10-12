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
			PossibleSecrets = new bool[GetMaxSecretCount(heroClass)];

			for(var i = 0; i < PossibleSecrets.Length; i++)
				PossibleSecrets[i] = true;
		}

		public int Id { get; private set; }
		public bool Stolen { get; private set; }
		public HeroClass HeroClass { get; private set; }
		public bool[] PossibleSecrets { get; set; }

		public static int GetMaxSecretCount(HeroClass heroClass)
		{
			switch(heroClass)
			{
				case HeroClass.Hunter:
					return CardIds.SecretIdsHunter.Count;
				case HeroClass.Mage:
					return CardIds.SecretIdsMage.Count;
				case HeroClass.Paladin:
					return CardIds.SecretIdsPaladin.Count;
				default:
					return 0;
			}
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

		public static int GetSecretIndex(HeroClass heroClass, string cardId)
		{
			switch(heroClass)
			{
				case HeroClass.Hunter:
					return CardIds.SecretIdsHunter.IndexOf(cardId);
				case HeroClass.Mage:
					return CardIds.SecretIdsMage.IndexOf(cardId);
				case HeroClass.Paladin:
					return CardIds.SecretIdsPaladin.IndexOf(cardId);
			}
			return -1;
		}
	}
}