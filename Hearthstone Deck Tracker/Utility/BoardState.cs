using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class BoardState
	{
		public PlayerBoard Player { get; private set; }
		public PlayerBoard Opponent { get; private set; }

		public BoardState()
		{
		}
	}

	public class PlayerBoard
	{
		public int Damage { get; set; }
		public List<CardEntity> Cards { get; set; }
	}
}
