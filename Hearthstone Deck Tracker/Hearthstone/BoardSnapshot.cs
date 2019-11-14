using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class BoardSnapshot
	{
		public BoardSnapshot(Entity[] entities, int turn)
		{
			Entities = entities;
			Turn = turn;
		}
		public Entity[] Entities { get; }
		public int Turn { get; }
	}
}
