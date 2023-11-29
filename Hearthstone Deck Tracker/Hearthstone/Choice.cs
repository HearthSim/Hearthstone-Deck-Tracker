using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using HearthDb.Enums;
using System;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	[Serializable]
	public class Choice
	{
		public readonly int Id;
		public readonly ChoiceType ChoiceType;
		public readonly List<Entity> ChosenEntities = new List<Entity>();

		public Choice(int id, ChoiceType choiceType)
		{
			Id = id;
			ChoiceType = choiceType;
		}

		public void AttachChosenEntity(int index, Entity entity)
		{
			ChosenEntities.Add(entity);
		}
	}
}
