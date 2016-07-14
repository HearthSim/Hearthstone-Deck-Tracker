#region

using System;
using HearthDb.Enums;
using static HearthDb.Enums.GameTag;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class LogReaderHelper
	{
		public static int ParseTag(GameTag tag, string rawValue)
		{
			switch(tag)
			{
				case ZONE:
					return (int)ParseEnum<Zone>(rawValue);
				case MULLIGAN_STATE:
					return (int)ParseEnum<Mulligan>(rawValue);
				case PLAYSTATE:
					return (int)ParseEnum<PlayState>(rawValue);
				case CARDTYPE:
					return (int)ParseEnum<CardType>(rawValue);
				case CLASS:
					return (int)ParseEnum<CardClass>(rawValue);
				case STATE:
					return (int)ParseEnum<State>(rawValue);
				case STEP:
					return (int)ParseEnum<Step>(rawValue);
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