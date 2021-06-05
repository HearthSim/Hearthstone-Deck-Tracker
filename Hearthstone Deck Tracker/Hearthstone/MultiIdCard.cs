using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

public class MultiIdCard
{
	public readonly string[] Ids;

	public MultiIdCard(params string[] ids)
	{
		Debug.Assert(ids.Distinct().Count() == ids.Length, "ids param contains duplicate values");
		Ids = ids;
	}

	private Card[] _cards = null;
	public Card[] Cards => _cards ?? (_cards = Ids.Select(Database.GetCardFromId).ToArray());

	public bool IsWild => Cards.Any(x => !Helper.ClassicOnlySets.Contains(x.Set));
	public bool IsClassic => Cards.Any(x => Helper.ClassicOnlySets.Contains(x.Set));
	public bool IsStandard => Cards.Any(x => !Helper.WildOnlySets.Contains(x.Set) && !Helper.ClassicOnlySets.Contains(x.Set));
	public bool HasSet(CardSet set) => Cards.Any(x => x.CardSet == set);

	public Card GetCardForFormat(Format? format) => GetCardForFormat(HearthDbConverter.GetFormatType(format));
	public Card GetCardForFormat(FormatType format)
	{
		switch(format)
		{
			case FormatType.FT_WILD:
				return (Cards.FirstOrDefault(x => !Helper.ClassicOnlySets.Contains(x.Set)) ?? Cards[0])?.Clone() as Card;
			case FormatType.FT_CLASSIC:
				return (Cards.FirstOrDefault(x => Helper.ClassicOnlySets.Contains(x.Set)) ?? Cards[0])?.Clone() as Card;
			case FormatType.FT_STANDARD:
				return (Cards.FirstOrDefault(x => !Helper.WildOnlySets.Contains(x.Set) && !Helper.ClassicOnlySets.Contains(x.Set)) ?? Cards[0])?.Clone() as Card;
			default:
				return Cards[0]?.Clone() as Card;
		}
	}

	public bool Equals(MultiIdCard card) => Equals(card.Ids);
	public bool Equals(string[] ids) => ids.All(Ids.Contains);
	public bool Equals(string id) => Ids.Contains(id);
	public override bool Equals(object obj)
	{
		if (obj is MultiIdCard card) 
			return Equals(card);
		if (obj is string str) 
			return Equals(str);
		if (obj is string[] arr) 
			return Equals(arr);
		return false;
	}

	public override int GetHashCode()
	{
		return 1885957745 + EqualityComparer<string[]>.Default.GetHashCode(Ids);
	}

	public static bool operator ==(MultiIdCard card, object other) => card.Equals(other);
	public static bool operator !=(MultiIdCard card, object other) => !card.Equals(other);

	public static implicit operator MultiIdCard(string id) => new MultiIdCard(id);

	public override string ToString()
	{
		return Cards[0].Name;
	}
}

public class QuantifiedMultiIdCard : MultiIdCard
{
	public QuantifiedMultiIdCard(MultiIdCard baseCard, int count) : base(baseCard.Ids)
	{
		Count = count;
	}

	public int Count { get; }
}
