using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone;

public static class CardUtils
{
	public static IEnumerable<Card?> FilterCardsByFormat(this IEnumerable<Card?> cards, Format? format)
	{
		return cards.Where(card => IsCardFromFormat(card, format));
	}

	public static bool IsCardFromFormat(Card? card, Format? format)
	{
		return format switch
		{
			Format.Classic => card != null && Helper.ClassicOnlySets.Contains(card.Set),
			Format.Wild => card != null && !Helper.ClassicOnlySets.Contains(card.Set),
			Format.Standard => card != null && !Helper.WildOnlySets.Contains(card.Set) && !Helper.ClassicOnlySets.Contains(card.Set),
			Format.Twist => card != null && Helper.TwistSets.Contains(card.Set),
			_ => true
		};
	}

	public static IEnumerable<Card?> FilterCardsByPlayerClass(this IEnumerable<Card?> cards, string? playerClass, bool ignoreNeutral = false)
	{
		return cards.Where(card => IsCardFromPlayerClass(card, playerClass, ignoreNeutral));
	}

	public static bool IsCardFromPlayerClass(Card? card, string? playerClass, bool ignoreNeutral = false)
	{
		return card != null &&
		       (card.PlayerClass == playerClass || card.GetTouristVisitClass() == playerClass ||
		        (!ignoreNeutral && card.CardClass == CardClass.NEUTRAL));
	}

	public static bool MayCardBeRelevant(Card? card, Format? format, string? playerClass,
		bool ignoreNeutral = false)
	{
		return IsCardFromFormat(card, format) && IsCardFromPlayerClass(card, playerClass, ignoreNeutral);
	}
}
