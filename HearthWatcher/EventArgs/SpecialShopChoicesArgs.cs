using System;
using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.EventArgs;

public class SpecialShopChoicesArgs : System.EventArgs
{
	public bool IsActive { get; }
	public List<BoardCard> BoardCards { get; }
	public int MousedOverSlot { get; }

	public SpecialShopChoicesArgs(bool isActive, List<BoardCard> boardCards, int mousedOverSlot)
	{
		IsActive = isActive;
		BoardCards = boardCards ?? new List<BoardCard>();
		MousedOverSlot = mousedOverSlot;
	}

	public override bool Equals(object? obj)
	{
		if (obj is not SpecialShopChoicesArgs other)
			return false;

		if (IsActive != other.IsActive)
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
		hash ^= IsActive.GetHashCode();
		foreach (var card in BoardCards)
		{
			hash ^= card?.EntityId?.GetHashCode() ?? 0;
			hash ^= (card?.Hovered ?? false).GetHashCode();
		}
		return hash;
	}
}
