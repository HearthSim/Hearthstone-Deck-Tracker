using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.Secrets
{
	public class Secret
	{
		public Entity Entity { get; }

		public Secret(Entity entity)
		{
			if(!entity.IsSecret)
				throw new ArgumentException(nameof(entity) + " is not a secret");
			if(!entity.HasTag(GameTag.CLASS))
				throw new ArgumentException(nameof(entity) + " has no CardClass");

			Entity = entity;
			Excluded = GetAllSecrets().ToDictionary(x => x, x => false);
		}

		public Dictionary<string, bool> Excluded { get; set; }

		public void Exclude(string cardId)
		{
			if(Excluded.ContainsKey(cardId))
				Excluded[cardId] = true;
		}

		private IEnumerable<string> GetAllSecrets()
		{
			switch(Entity.GetTag(GameTag.CLASS))
			{
				case (int)CardClass.HUNTER:
					return CardIds.Secrets.Hunter.All;
				case (int)CardClass.MAGE:
					return CardIds.Secrets.Mage.All;
				case (int)CardClass.PALADIN:
					return CardIds.Secrets.Paladin.All;
				case (int)CardClass.ROGUE:
					return CardIds.Secrets.Rogue.All;
				default:
					return new List<string>();
			}
		}

		public bool IsExcluded(string cardId) => Excluded.TryGetValue(cardId, out var excluded) && excluded;

		public void Include(string cardId)
		{
			if(Excluded.ContainsKey(cardId))
				Excluded[cardId] = false;
		}
	}
}

