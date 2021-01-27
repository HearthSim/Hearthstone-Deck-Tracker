namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class PredictedCard
	{
		public string CardId { get; set; }
		public int Turn { get; set; }
		public bool IsCreated { get; set; }

		public PredictedCard(string cardId, int turn, bool isCreated = false)
		{
			CardId = cardId;
			Turn = turn;
			IsCreated = isCreated;
		}
	}
}
