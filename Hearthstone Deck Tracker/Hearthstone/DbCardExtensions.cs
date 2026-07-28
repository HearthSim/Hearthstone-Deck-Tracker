using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone;

public static class DbCardExtensions
{
	public static int GetTag(this HearthDb.Card card, GameTag gameTag) => card.Entity.GetTag(gameTag);

	public static bool HasTag(this HearthDb.Card card, GameTag gameTag) => card.GetTag(gameTag) > 0;

	public static bool HasDeathrattle(this HearthDb.Card card) => card.Mechanics?.Contains("Deathrattle") ?? false;

	public static bool HasTaunt(this HearthDb.Card card) => card.Mechanics?.Contains("Taunt") ?? false;

	public static IEnumerable<string> GetClasses(this HearthDb.Card card)
	{
		var multipleClasses = card.Entity.GetTag(GameTag.MULTIPLE_CLASSES);
		if(multipleClasses == 0)
		{
			yield return HearthDbConverter.ConvertClass(card.Class) ?? "Neutral";
			yield break;
		}

		var cardClass = 1;
		while(multipleClasses != 0)
		{
			if(1 == (multipleClasses & 1))
			{
				var className = HearthDbConverter.ConvertClass((CardClass)cardClass);
				yield return className ?? "Neutral";
			}
			multipleClasses >>= 1;
			cardClass++;
		}
	}

	public static bool IsClass(this HearthDb.Card card, string? playerClass)
	{
		if(playerClass == null)
			return false;
		return card.GetClasses().Contains(playerClass);
	}

	public static bool HasRace(this HearthDb.Card card, Race race)
	{
		if(race == card.Race || race == card.SecondaryRace)
			return true;
		if(race == Race.BLANK)
			return false;
		if(card.Race == Race.ALL || card.SecondaryRace == Race.ALL)
			return true;
		return false;
	}

	public static bool IsMech(this HearthDb.Card card) => card.HasRace(Race.MECHANICAL);

	public static bool IsDemon(this HearthDb.Card card) => card.HasRace(Race.DEMON);

	public static bool IsBeast(this HearthDb.Card card) => card.HasRace(Race.BEAST);

	public static bool IsMurloc(this HearthDb.Card card) => card.HasRace(Race.MURLOC);

	public static bool IsDragon(this HearthDb.Card card) => card.HasRace(Race.DRAGON);

	public static bool IsPirate(this HearthDb.Card card) => card.HasRace(Race.PIRATE);

	public static bool IsElemental(this HearthDb.Card card) => card.HasRace(Race.ELEMENTAL);

	public static bool IsQuillboar(this HearthDb.Card card) => card.HasRace(Race.QUILBOAR);

	public static bool IsUndead(this HearthDb.Card card) => card.HasRace(Race.UNDEAD);

	public static bool IsNaga(this HearthDb.Card card) => card.HasRace(Race.NAGA);

	public static bool isDraenei(this HearthDb.Card card) => card.HasRace(Race.DRAENEI);
}
