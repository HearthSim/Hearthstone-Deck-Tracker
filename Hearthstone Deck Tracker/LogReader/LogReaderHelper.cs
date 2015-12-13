#region

using System;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class LogReaderHelper
	{
		public static int ParseTagValue(GAME_TAG tag, string rawValue)
		{
			int value;
			switch(tag)
			{
				case GAME_TAG.ZONE:
					TAG_ZONE zone;
					Enum.TryParse(rawValue, out zone);
					value = (int)zone;
					break;
				case GAME_TAG.MULLIGAN_STATE:
				{
					TAG_MULLIGAN state;
					Enum.TryParse(rawValue, out state);
					value = (int)state;
				}
					break;
				case GAME_TAG.PLAYSTATE:
				{
					TAG_PLAYSTATE state;
					Enum.TryParse(rawValue, out state);
					value = (int)state;
				}
					break;
				case GAME_TAG.CARDTYPE:
					TAG_CARDTYPE type;
					Enum.TryParse(rawValue, out type);
					value = (int)type;
					break;
				case GAME_TAG.CLASS:
					TAG_CLASS @class;
					Enum.TryParse(rawValue, out @class);
					value = (int)@class;
					break;
				default:
					int.TryParse(rawValue, out value);
					break;
			}
			return value;
		}
	}
}