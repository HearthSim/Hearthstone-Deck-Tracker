using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone;

public abstract record BattlegroundsKeyword
{
	public abstract string Name { get; }

	public abstract bool Matches(Func<GameTag, int> getTag, string? englishText);
}

/// <summary>
/// A keyword the client does not expose as a tag, recognized by its mention in the card text.
/// Always matches against English so the filter does not depend on the language the app runs in.
/// </summary>
public record MentionedKeyword(string LocKey) : BattlegroundsKeyword
{
	public override string Name => LocUtil.Get(LocKey);

	public override bool Matches(Func<GameTag, int> getTag, string? englishText)
	{
		// a missing string would turn into a substring match on "", matching every card
		var mention = LocUtil.GetEnglish(LocKey);
		return !string.IsNullOrEmpty(mention) && (englishText?.Contains(mention) ?? false);
	}
}

public sealed record TagKeyword(GameTag Tag, string LocKey) : MentionedKeyword(LocKey)
{
	public override bool Matches(Func<GameTag, int> getTag, string? englishText) =>
		getTag(Tag) > 0 || base.Matches(getTag, englishText);
}
