using System;
using Hearthstone_Deck_Tracker.API;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

/// <summary>
/// "Fill your board" effects summon minions into the empty slots of the 7-slot board.
/// Minions and locations already in play occupy slots (Entity.TakesBoardSlot), so the fill
/// count is 7 minus the current occupants. At least one slot is assumed so the summary stays
/// meaningful at a full board (where the card would not usually be played anyway).
/// </summary>
internal static class BoardFill
{
	private const int MaxBoardSize = 7;

	/// <summary>Empty slots on the local player's board.</summary>
	public static int PlayerSlots => Math.Max(1, MaxBoardSize - Core.Game.PlayerBoardCount);

	/// <summary>Empty slots on the opponent's board.</summary>
	public static int OpponentSlots => Math.Max(1, MaxBoardSize - Core.Game.OpponentBoardCount);
}
