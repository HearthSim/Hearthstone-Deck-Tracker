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

		public int Turn
		{
			get { return Data[0].GetTag(GAME_TAG.TURN); }
		}

		[Obsolete]
		public override string ToString()
		{
			var additionalInfo = "";
			if(Type == KeyPointType.Attack)
			{
				var attackerId = Data[0].GetTag(GAME_TAG.PROPOSED_ATTACKER);
				var attackerCardId = Data.First(x => x.Id == attackerId).CardId;
				if(!string.IsNullOrEmpty(attackerCardId))
					additionalInfo += Database.GetCardFromId(attackerCardId).LocalizedName;

				additionalInfo += " -> ";

				var defenderId = Data[0].GetTag(GAME_TAG.PROPOSED_DEFENDER);
				var defenderCardId = Data.First(x => x.Id == defenderId).CardId;
				if(!string.IsNullOrEmpty(defenderCardId))
					additionalInfo += Database.GetCardFromId(defenderCardId).LocalizedName;
			}
			else if(Type == KeyPointType.PlaySpell)
			{
				var entity = Data.First(x => x.Id == Id);
				if(!string.IsNullOrEmpty(entity.CardId))
					additionalInfo += Database.GetCardFromId(entity.CardId).LocalizedName;

				additionalInfo += " -> ";

				var targetId = entity.GetTag(GAME_TAG.CARD_TARGET);
				var targetCardId = Data.First(x => x.Id == targetId).CardId;
				if(!string.IsNullOrEmpty(targetCardId))
					additionalInfo += Database.GetCardFromId(targetCardId).LocalizedName;
			}
			else
			{
				var entityCardId = Data.First(x => x.Id == Id).CardId;
				if(!string.IsNullOrEmpty(entityCardId))
					additionalInfo = Database.GetCardFromId(entityCardId).LocalizedName;
				else
					additionalInfo = "Entity " + Id;
			}
			return string.Format("{1} {2}", Player, Type, additionalInfo);
		}

		public string GetCardId()
		{
			var id = Type == KeyPointType.Attack ? Data[0].GetTag(GAME_TAG.PROPOSED_ATTACKER) : Id;
			var entity = Data.FirstOrDefault(x => x.Id == id);
			return entity != null ? entity.CardId : null;
		}

		public string GetAdditionalInfo()
		{
			if(Type == KeyPointType.Victory || Type == KeyPointType.Defeat)
				return Type.ToString();
			var cardId = GetCardId();
			return string.IsNullOrEmpty(cardId) ? "Entity " + Id : Database.GetCardFromId(GetCardId()).LocalizedName;
		}
	}
}