#region

using System;
using HearthDb.Enums;
using static HearthDb.Enums.GameTag;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class GameTagHelper
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
				  int.TryParse(rawValue, out var value);
					return value;
			}
		}

		public static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
		    if(Enum.TryParse<TEnum>(value, out var tEnum))
				return tEnum;
			if (int.TryParse(value, out var i) && Enum.IsDefined(typeof(TEnum), i))
			  tEnum = (TEnum)(object)i;
			return tEnum;
		}
	}
}
