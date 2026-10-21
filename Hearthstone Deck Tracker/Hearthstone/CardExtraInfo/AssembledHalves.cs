using System.Text.RegularExpressions;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.CardExtraInfo;

/// <summary>
/// The halves of an assembled card: a trigger (first half) and an effect (second half), each the
/// dbf id of the card the half was taken from, or 0 when the card keeps its own half.
/// </summary>
public class AssembledHalves : ICardExtraInfo
{
	// Card text marks where the trigger half ends and the effect half begins.
	private const char HalfSeparator = '&';

	public int FirstHalfDbfId { get; }

	public int SecondHalfDbfId { get; }

	public string? CardNameSuffix => "*";

	public AssembledHalves(int firstHalfDbfId, int secondHalfDbfId)
	{
		FirstHalfDbfId = firstHalfDbfId;
		SecondHalfDbfId = secondHalfDbfId;
	}

	/// <summary>
	/// The halves an entity was assembled with, or null when it is not assembled.
	/// </summary>
	public static AssembledHalves? FromEntity(Entity entity)
	{
		var first = entity.GetTag(GameTag.CRAFTABLE_FIRST_HALF_DBID);
		var second = entity.GetTag(GameTag.CRAFTABLE_SECOND_HALF_DBID);
		// 1 marks a choice entity on the Assemble screen, not a finished card.
		if(first == 1 || second == 1 || (first <= 1 && second <= 1))
			return null;
		return new AssembledHalves(first > 1 ? first : 0, second > 1 ? second : 0);
	}

	/// <summary>
	/// The card whose effect (second half) the assembled card has.
	/// </summary>
	public string EffectCardId(string baseCardId)
		=> SecondHalfDbfId > 0 ? Database.GetCardFromDbfId(SecondHalfDbfId, false)?.Id ?? baseCardId : baseCardId;

	/// <summary>
	/// The assembled card text: the trigger half's text up to the separator, then the effect half's
	/// text after it, keeping the bold and italic tags. Null when the text of either half can't be found.
	/// </summary>
	public string? GetText(Card baseCard)
	{
		var trigger = GetHalfText(FirstHalfDbfId > 0 ? Database.GetCardFromDbfId(FirstHalfDbfId, false) : baseCard, 0);
		var effect = GetHalfText(SecondHalfDbfId > 0 ? Database.GetCardFromDbfId(SecondHalfDbfId, false) : baseCard, 1);
		if(trigger == null || effect == null)
			return null;
		return $"{trigger} {effect}".Trim();
	}

	private static string? GetHalfText(Card? card, int half)
	{
		var parts = card?.FormattedText.Split(new[] { HalfSeparator }, 2);
		if(parts is not { Length: 2 })
			return null;
		// Card text is broken into lines for the card frame, which don't apply once halves are joined.
		return Regex.Replace(parts[half], @"\s+", " ").Trim();
	}

	public bool Equals(ICardExtraInfo? other) => other is AssembledHalves halves
	                                             && FirstHalfDbfId == halves.FirstHalfDbfId
	                                             && SecondHalfDbfId == halves.SecondHalfDbfId;

	public override bool Equals(object? other) => Equals(other as AssembledHalves);

	public override int GetHashCode()
	{
		unchecked
		{
			return (FirstHalfDbfId * 397) ^ SecondHalfDbfId;
		}
	}

	public object Clone() => new AssembledHalves(FirstHalfDbfId, SecondHalfDbfId);
}
