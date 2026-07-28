using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.EventArgs;

/// <summary>
/// One side's play zone. In Battlegrounds the opposing zone is Bob's shop.
/// </summary>
public class PlayZoneArgs
{
	public List<BoardCard> BoardCards { get; }
	public int MousedOverSlot { get; }

	public PlayZoneArgs(List<BoardCard> boardCards, int mousedOverSlot)
	{
		BoardCards = boardCards ?? new List<BoardCard>();
		MousedOverSlot = mousedOverSlot;
	}

	public override bool Equals(object? obj)
	{
		if(obj is not PlayZoneArgs other)
			return false;

		if(MousedOverSlot != other.MousedOverSlot)
			return false;

		if(BoardCards.Count != other.BoardCards.Count)
			return false;

		for(var i = 0; i < BoardCards.Count; i++)
		{
			var thisCard = BoardCards[i];
			var otherCard = other.BoardCards[i];
			if(thisCard?.EntityId != otherCard?.EntityId)
				return false;
			if(thisCard?.ZonePosition != otherCard?.ZonePosition)
				return false;
			if((thisCard?.Hovered ?? false) != (otherCard?.Hovered ?? false))
				return false;
		}

		return true;
	}

	public override int GetHashCode()
	{
		var hash = MousedOverSlot.GetHashCode();
		foreach(var card in BoardCards)
		{
			hash ^= card?.EntityId?.GetHashCode() ?? 0;
			hash ^= card?.ZonePosition.GetHashCode() ?? 0;
			hash ^= (card?.Hovered ?? false).GetHashCode();
		}
		return hash;
	}
}

public class BoardStateArgs : System.EventArgs
{
	public PlayZoneArgs? Friendly { get; }
	public PlayZoneArgs? Opposing { get; }

	public BoardStateArgs(PlayZoneArgs? friendly, PlayZoneArgs? opposing)
	{
		Friendly = friendly;
		Opposing = opposing;
	}

	public override bool Equals(object? obj)
	{
		if(obj is not BoardStateArgs other)
			return false;
		return Equals(Friendly, other.Friendly) && Equals(Opposing, other.Opposing);
	}

	public override int GetHashCode()
		=> (Friendly?.GetHashCode() ?? 0) ^ (Opposing?.GetHashCode() ?? 0);
}
