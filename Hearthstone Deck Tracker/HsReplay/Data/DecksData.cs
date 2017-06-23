namespace Hearthstone_Deck_Tracker.HsReplay.Data
{
	public class DecksData : HsReplayData
	{
		public string[] Decks { get; set; } = new string[0];

		public override string ToString()
		{
			return $"Count={Decks.Length}, ServerTS={ServerTimeStamp}, Downloaded={ClientTimeStamp} Age={Age}";
		}
	}
}
