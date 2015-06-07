#region

using Hearthstone_Deck_Tracker.Enums;

#endregion

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
		void HandlePlayerPlayToDeck(string cardId, int turn);
		void HandlePlayerHeroPower(string cardId, int turn);
		void SetPlayerHero(string playerHero);
		void HandlePlayerName(string name);
		void HandlePlayerGetToDeck(string cardId, int turn);
		void TurnStart(ActivePlayer player, int turnNumber);
		void HandleGameStart();
		void HandleGameEnd();
		void HandleLoss();
		void HandleWin();
		void HandleTied();
		void PlayerSetAside(string id);
		void ResetConstructedImporting();
		void HandlePossibleConstructedCard(string id, bool canBeDoneImporting);
		void HandlePossibleArenaCard(string id);
		void SetGameMode(GameMode mode);
		void HandleInMenu();
		void HandleConcede();
		void HandlePlayerFatigue(int currentDamage);
		void HandleOpponentFatigue(int currentDamage);
		void SetRank(int rank);
		void HandleDustReward(int amount);
		void HandleGoldReward(int amount);

		#region OpponentHandlers

		void HandleOpponentPlay(string cardId, int from, int turn);
		void HandleOpponentHandDiscard(string cardId, int from, int turn);
		void HandleOpponentDraw(int turn);
		void HandleOpponentMulligan(int from);
		void HandleOpponentGet(int turn, int id);
		void HandleOpponentSecretPlayed(string cardId, int from, int turn, bool fromDeck, int otherId);
		void HandleOpponentPlayToHand(string cardId, int turn, int id);
		void HandleOpponentPlayToDeck(string cardId, int turn);
		void HandleOpponentSecretTrigger(string cardId, int turn, int otherId);
		void HandleOpponentDeckDiscard(string cardId, int turn);
		void SetOpponentHero(string hero);
		void HandleOpponentHeroPower(string cardId, int turn);
		void HandleOpponentName(string name);
		void HandleOpponentGetToDeck(int turn);

		#endregion OpponentHandlers
	}
}