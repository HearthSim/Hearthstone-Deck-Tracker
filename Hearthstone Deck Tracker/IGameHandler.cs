namespace Hearthstone_Deck_Tracker
{

	public interface IGameHandler
	{
		void HandlePlayerGet(string cardId, int turn);
		void HandlePlayerBackToHand(string cardId, int turn);
		void HandlePlayerDraw(string cardId, int turn);
		void HandlePlayerMulligan(string cardId);
		void HandlePlayerSecretPlayed(string cardId, int turn, bool fromDeck);
		void HandlePlayerHandDiscard(string cardId, int turn);
		void HandlePlayerPlay(string cardId, int turn);
		void HandlePlayerDeckDiscard(string cardId, int turn);
		void HandlePlayerHeroPower(string cardId, int turn);

		#region OpponentHandlers

		void HandleOpponentPlay(string cardId, int from, int turn);
		void HandleOpponentHandDiscard(string cardId, int from, int turn);
		void HandleOpponentDraw(int turn);
		void HandleOpponentMulligan(int from);
		void HandleOpponentGet(int turn);
		void HandleOpponentSecretPlayed(string cardId, int from, int turn, bool fromDeck);
		void HandleOpponentPlayToHand(string cardId, int turn);
		void HandleOpponentSecretTrigger(string cardId, int turn);
		void HandleOpponentDeckDiscard(string cardId, int turn);
		void SetOpponentHero(string hero);
		void HandleOpponentHeroPower(string cardId, int turn);
	  
		#endregion OpponentHandlers

		void TurnStart(Turn player, int turnNumber);
		void HandleGameStart(string playerHero);
		void HandleGameEnd(bool backInMenu);
		void HandleLoss();
		void HandleWin();
		void PlayerSetAside(string id);


	}
}
