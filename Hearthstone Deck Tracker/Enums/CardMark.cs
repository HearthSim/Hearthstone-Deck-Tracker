using Hearthstone_Deck_Tracker.Utility.Attributes;

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum CardMark
	{
		[AssetName(null)]
		None = ' ',

		[AssetName("/Images/card-icon-coin.png")]
		Coin = 'c',

		[AssetName("/Images/card-icon-returned.png")]
		Returned = 'R',

		[AssetName("/Images/card-icon-mulligan.png")]
		Mulliganed = 'M',

		[AssetName("/Images/card-icon-created.png")]
		Created = 'C',

		[AssetName("/Images/card-icon-keep.png")]
		Kept = 'K'
	}
}
