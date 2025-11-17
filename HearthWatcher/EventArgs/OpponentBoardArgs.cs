using System;
using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.EventArgs;

public class OpponentBoardArgs : System.EventArgs
{
	public List<BoardCard> BoardCards { get; }
	public int MousedOverSlot { get; }

	public OpponentBoardArgs(List<BoardCard> boardCards, int mousedOverSlot)
	{
		BoardCards = boardCards ?? new List<BoardCard>();
		MousedOverSlot = mousedOverSlot;
	}

	public override bool Equals(object? obj)
	{
		if (obj is not OpponentBoardArgs other)
			return false;

		if (MousedOverSlot != other.MousedOverSlot)
			return false;

		if (BoardCards.Count != other.BoardCards.Count)
			return false;

		for (int i = 0; i < BoardCards.Count; i++)
		{
			var thisCard = BoardCards[i];
			var otherCard = other.BoardCards[i];
			if (thisCard?.EntityId != otherCard?.EntityId)
				return false;
			if ((thisCard?.Hovered ?? false) != (otherCard?.Hovered ?? false))
				return false;
		}

		return true;
	}

	public override int GetHashCode()
	{
		var hash = MousedOverSlot.GetHashCode();
		foreach (var card in BoardCards)
		{
			hash ^= card?.EntityId?.GetHashCode() ?? 0;
			hash ^= (card?.Hovered ?? false).GetHashCode();
		}
		return hash;
	}
}
