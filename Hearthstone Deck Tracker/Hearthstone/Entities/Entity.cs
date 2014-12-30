using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;

namespace Hearthstone_Deck_Tracker.Hearthstone.Entities
{
	public class Entity
	{
		public Dictionary<GAME_TAG, int> Tags { get; set; }
		public TAG_ZONE Zone { get; set; }
		public string Name { get; set; }
		public int Id { get; set; }
		public string CardId { get; set; }
		public string Type { get; set; }
		public int ZonePos { get; set; }
		public int Player { get; set; }
		public bool IsPlayer { get; set; }

		public Entity()
		{
			
		}
		public Entity(int id)
		{
			Tags = new Dictionary<GAME_TAG, int>();
			Id = id;
		}
		public bool HasTag(GAME_TAG tag)
		{
			return GetTag(tag) > 0;
		}

		public int GetTag(GAME_TAG tag)
		{
			int value;
			Tags.TryGetValue(tag, out value);
			return value;
		}

		public void SetTag(GAME_TAG tag, int value)
		{
			var prevVal = 0;
			if(!Tags.ContainsKey(tag))
				Tags.Add(tag, value);
			else
			{
				prevVal = Tags[tag];
				Tags[tag] = value;
			}

			if(tag == GAME_TAG.ZONE)
				Zone = (TAG_ZONE)value;
			
			//Logger.WriteLine(string.Format("[id={0} cardId={1} name={2} TAG={3}] {4} -> {5}", Id, CardId, Name, tag, prevVal, value));
		}
	}
}
