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

		public Dictionary<MultiIdCard, bool> Excluded { get; set; }

		public void Exclude(MultiIdCard cardId)
		{
			if(Excluded.ContainsKey(cardId) && !Entity.HasTag(GameTag.SECRET_LOCKED))
				Excluded[cardId] = true;
		}

		private IEnumerable<MultiIdCard> GetAllSecrets()
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
					return new List<MultiIdCard>();
			}
		}

		public bool IsExcluded(MultiIdCard cardId) => Excluded.TryGetValue(cardId, out var excluded) && excluded;

		public void Include(MultiIdCard cardId)
		{
			if(Excluded.ContainsKey(cardId))
				Excluded[cardId] = false;
		}
	}
}

