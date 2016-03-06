#region

using System;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using static Hearthstone_Deck_Tracker.Enums.GAME_TAG;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class LogReaderHelper
	{
		public static int ParseTag(GAME_TAG tag, string rawValue)
		{
			switch(tag)
			{
				case ZONE:
					return (int)ParseEnum<TAG_ZONE>(rawValue);
				case MULLIGAN_STATE:
					return (int)ParseEnum<TAG_MULLIGAN>(rawValue);
				case PLAYSTATE:
					return (int)ParseEnum<TAG_PLAYSTATE>(rawValue);
				case CARDTYPE:
					return (int)ParseEnum<TAG_CARDTYPE>(rawValue);
				case CLASS:
					return (int)ParseEnum<TAG_CLASS>(rawValue);
				default:
					int value;
					int.TryParse(rawValue, out value);
					return value;
			}
		}

		public static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			TEnum tEnum;
			if(Enum.TryParse(value, out tEnum))
				return tEnum;
			int i;
			if(int.TryParse(value, out i) && Enum.IsDefined(typeof(TEnum), i))
				tEnum = (TEnum)(object)i;
			return tEnum;
		}
	}
}