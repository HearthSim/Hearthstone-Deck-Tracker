using System.Runtime.Serialization;

namespace Hearthstone_Deck_Tracker.Live.Data
{
	public enum DataType
	{
		[EnumMember(Value = "board_state")]
		BoardState,

		[EnumMember(Value = "game_end")]
		GameEnd
	}
}
