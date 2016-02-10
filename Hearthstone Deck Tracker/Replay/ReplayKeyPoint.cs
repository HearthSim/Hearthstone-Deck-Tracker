#region

using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

#endregion

namespace Hearthstone_Deck_Tracker.Replay
{
	public class ReplayKeyPoint
	{
		public Entity[] Data;
		public int Id;
		public ActivePlayer Player;
		public KeyPointType Type;

		public ReplayKeyPoint(Entity[] data, KeyPointType type, int id, ActivePlayer player)
		{
			if(data != null)
				Data = Helper.DeepClone(data);
			Type = type;
			Id = id;
			Player = player;
		}

		public int Turn => Data[0].GetTag(GAME_TAG.TURN);
		
		public string GetCardId()
		{
			var id = Type == KeyPointType.Attack ? Data[0].GetTag(GAME_TAG.PROPOSED_ATTACKER) : Id;
			return Data.FirstOrDefault(x => x.Id == id)?.CardId;
		}

		public string GetAdditionalInfo()
		{
			if(Type == KeyPointType.Victory || Type == KeyPointType.Defeat)
				return Type.ToString();
			return string.IsNullOrEmpty(GetCardId()) ? "Entity " + Id : Database.GetCardFromId(GetCardId()).LocalizedName;
		}
	}
}