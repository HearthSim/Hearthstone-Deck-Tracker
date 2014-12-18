namespace Hearthstone_Deck_Tracker
{
	public class Secret
	{
		public string CardId { get; private set; }
		public int Count { get; set; }

		public Secret(string cardId, int count)
		{
			CardId = cardId;
			Count = count;
		}
	}
}