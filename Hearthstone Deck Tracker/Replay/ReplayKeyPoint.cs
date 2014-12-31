using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Replay
{
	public class ReplayKeyPoint
	{
		public Entity[] Data;
		public KeyPointType Type;
		public ActivePlayer Player;
		public int Id;

		public int Turn { get { return Data[0].GetTag(GAME_TAG.TURN); } }

		public ReplayKeyPoint(Entity[] data, KeyPointType type, int id)
		{
			Data = ReplayMaker.DeepClone(data);
			Type = type;
			Id = id;
		}

		public override string ToString()
		{
			return "Turn: " + Turn + " - " + Type + " " + Id;
		}
	}
}