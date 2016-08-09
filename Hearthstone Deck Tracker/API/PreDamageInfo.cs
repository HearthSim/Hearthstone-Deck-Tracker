using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.API
{
	public class PredamageInfo
	{
		public Entity Entity { get; }
		public int Value { get; }

		public PredamageInfo(Entity entity, int value)
		{
			Entity = entity;
			Value = value;
		}
	}
}
