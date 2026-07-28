using System.Collections.Concurrent;
using System.Linq;
using HearthDb.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

/// <summary>
/// Resolves the game type, format and player class a discover pool should be built for.
///
/// During a match these come straight from the live game. In the menu there is no game:
/// Player.CurrentClass has been cleared by Player.Reset and CurrentGameType is GT_UNKNOWN, so the
/// pool would be built with no class and no format legality filtering. Outside a match we
/// substitute the selected deck's class plus the game type and format selected on the tournament
/// screen — the same deck picker signal the mulligan pre-lobby uses — and fall back to whatever
/// this deck last played.
/// </summary>
public static class PoolContext
{
	private static VisualsFormatType _menuVisualsFormatType = VisualsFormatType.VFT_UNKNOWN;

	public static VisualsFormatType MenuVisualsFormatType
	{
		get => _menuVisualsFormatType;
		set
		{
			if(_menuVisualsFormatType == value)
				return;
			_menuVisualsFormatType = value;
			PreloadLegalCards();
		}
	}

	private static readonly ConcurrentDictionary<(GameType, FormatType), bool> _requestedLegalCards = new();

	/// <summary>
	/// The legal card list is normally fetched once a match starts, for that match's game type and
	/// format. Menu pools resolve a pair before any game exists, so fetch that pair too - without
	/// it CardLegalityChecker misses its lookup table and falls back to filtering by card set
	/// rather than the live list. Fire and forget: the pool is built with the fallback until the
	/// request lands, exactly as it is in the opening seconds of a match.
	/// </summary>
	private static void PreloadLegalCards()
	{
		if(InGame)
			return;
		var gameType = GetGameType();
		var format = GetFormatType();
		if(gameType == GameType.GT_UNKNOWN || format == FormatType.FT_UNKNOWN)
			return;
		// One request per pair per session. In-game the list is refreshed every game start; here
		// there is no comparable boundary to refresh on.
		if(!_requestedLegalCards.TryAdd((gameType, format), true))
			return;
		CardLegalityChecker.LoadCardsByFormat(gameType, format).Forget();
	}

	private static bool InGame => Core.Game.CurrentGameType != GameType.GT_UNKNOWN;

	public static GameType GetGameType()
	{
		if(InGame)
			return Core.Game.CurrentGameType;

		var fromDeckPicker = MenuVisualsFormatType switch
		{
			VisualsFormatType.VFT_CASUAL => GameType.GT_CASUAL,
			VisualsFormatType.VFT_STANDARD or VisualsFormatType.VFT_WILD
				or VisualsFormatType.VFT_TWIST or VisualsFormatType.VFT_CLASSIC => GameType.GT_RANKED,
			_ => GameType.GT_UNKNOWN,
		};
		if(fromDeckPicker != GameType.GT_UNKNOWN)
			return fromDeckPicker;

		return LastGameOfActiveDeck?.GameType ?? GameType.GT_UNKNOWN;
	}

	public static FormatType GetFormatType()
	{
		if(InGame)
			return Core.Game.CurrentFormatType;

		var fromDeckPicker = MenuVisualsFormatType switch
		{
			VisualsFormatType.VFT_STANDARD => FormatType.FT_STANDARD,
			VisualsFormatType.VFT_WILD or VisualsFormatType.VFT_CASUAL => FormatType.FT_WILD,
			VisualsFormatType.VFT_TWIST => FormatType.FT_TWIST,
			VisualsFormatType.VFT_CLASSIC => FormatType.FT_CLASSIC,
			_ => FormatType.FT_UNKNOWN,
		};
		if(fromDeckPicker != FormatType.FT_UNKNOWN)
			return fromDeckPicker;

		var lastPlayed = HearthDbConverter.GetFormatType(LastGameOfActiveDeck?.Format);
		if(lastPlayed != FormatType.FT_UNKNOWN)
			return lastPlayed;

		return Core.Game.CurrentFormatType;
	}

	public static void ApplyMenuDefaults(Player player)
	{
		if(InGame)
			return;
		var deckClass = DeckList.Instance.ActiveDeckVersion?.Class;
		if(!string.IsNullOrEmpty(deckClass))
			player.CurrentClass = deckClass;
		PreloadLegalCards();
	}

	private static GameStats? LastGameOfActiveDeck =>
		DeckList.Instance.ActiveDeck?.DeckStats.Games
			.OrderByDescending(g => g.StartTime)
			.FirstOrDefault();
}
