using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Live.Data
{
	public static class PayloadFactory
	{
		public static Payload GameEnd()
		{
			return new Payload
			{
				Type = DataType.GameEnd,
				Data = new Dictionary<object, object>()
			};
		}

		public static Payload BoardState(BoardState boardState)
		{
			return new Payload
			{
				Type = DataType.BoardState,
				Data = boardState,
			};
		}

		public static Payload GameStart(GameStart gameStart)
		{
			return new Payload
			{
				Type = DataType.GameStart,
				Data = gameStart
			};
		}
	}
}
