#region

using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public interface IGameHandler
	{
		void HandlePlayerGet(Entity entity, string cardId, int turn);
		void HandlePlayerBackToHand(Entity entity, string cardId, int turn);
		void HandlePlayerDraw(Entity entity, string cardId, int turn);
		void HandlePlayerMulligan(Entity entity, string cardId);
		void HandlePlayerHandToDeck(Entity entity, string cardId, IHsGameState gameState);
		void HandlePlayerSecretPlayed(Entity entity, string cardId, int turn, Zone fromZone, string parentBlockCardId);
		void HandlePlayerHandDiscard(Entity entity, string cardId, int turn);
		void HandlePlayerPlay(Entity entity, string cardId, int turn, string parentBlockCardId);
		void HandlePlayerDeckDiscard(Entity entity, string cardId, int turn);
		void HandlePlayerPlayToDeck(Entity entity, string cardId, int turn);
		void HandlePlayerHeroPower(string cardId, int turn);
		void HandlePlayerEntityChoices(IHsChoice choice);
		void HandlePlayerEntitiesChosen(IHsCompletedChoice choice);
		void HandleOpponentEntitiesChosen(IHsCompletedChoice choice);
		void SetPlayerHero(string? cardId);
		void HandlePlayerGetToDeck(Entity entity, string cardId, int turn);
		void TurnStart(ActivePlayer player, int turnNumber);
		void HandleGameStart(DateTime startTime);
		void HandleGameReconnect(DateTime reconnectTime);
		void HandleGameEnd(bool stateComplete);
		void HandleLoss();
		void HandleWin();
		void HandleTied();
		void HandleInMenu();
		void HandleConcede();
		void HandlePlayerFatigue(int currentDamage);
		void HandleOpponentFatigue(int currentDamage);
		void HandlePlayerMaxHealthChange(int value);
		void HandleOpponentMaxHealthChange(int value);
		void HandlePlayerMaxManaChange(int value);
		void HandleOpponentMaxManaChange(int value);
		void HandlePlayerMaxHandSizeChange(int value);
		void HandleOpponentMaxHandSizeChange(int value);

		void HandleOpponentJoust(Entity entity, string? cardId, int turn);
		void HandlePlayerPlayToGraveyard(Entity entity, string cardId, int turn, bool playersTurn);
		void HandleOpponentPlayToGraveyard(Entity entity, string? cardId, int turn, bool playersTurn);
		void HandlePlayerCreateInPlay(Entity entity, string cardId, int turn);
		void HandleOpponentCreateInPlay(Entity entity, string? cardId, int turn);
		void HandlePlayerJoust(Entity entity, string cardId, int turn);
		void HandlePlayerHandToPlay(Entity entity, string? cardId, int turn);
		void HandleOpponentHandToPlay(Entity entity, string? cardId, int turn);
		void HandlePlayerDeckToPlay(Entity entity, string? cardId, int turn);
		void HandleOpponentDeckToPlay(Entity entity, string? cardId, int turn);
		void HandlePlayerRemoveFromDeck(Entity entity, int turn);
		void HandleOpponentRemoveFromDeck(Entity entity, int turn);
		void HandlePlayerStolen(Entity entity, string? cardId, int turn);
		void HandleOpponentStolen(Entity entity, string? cardId, int turn);
		void HandlePlayerRemoveFromPlay(Entity entity, int turn);
		void HandleOpponentRemoveFromPlay(Entity entity, int turn);
		void HandlePlayerCreateInSetAside(Entity entity, int getTurnNumber);
		void HandlePlayerDredge();
		void HandlePlayerUnknownCardAddedToDeck();
		void HandlePlayerSecretTrigger(Entity entity, string? cardId, int turn, int otherId);

		#region SecretTriggers

		void HandleAttackingEntity(Entity? entity);
		void HandleDefendingEntity(Entity? entity);
		void HandlePlayerMinionPlayed(Entity entity);
		void HandlePlayerMinionDeath(Entity entity);
		void HandleOpponentMinionDeath(Entity entity, int turn);
		void HandleTurnsInPlayChange(Entity entity, int turn);

		#endregion

		#region OpponentHandlers

		void HandleOpponentPlay(Entity entity, string? cardId, int from, int turn);
		void HandleOpponentHandDiscard(Entity entity, string? cardId, int from, int turn);
		void HandleOpponentDraw(Entity entity, int turn, string? cardId, int? drawerId);
		void HandleOpponentMulligan(Entity entity, int from);
		void HandleOpponentGet(Entity entity, int turn, int id);
		void HandleOpponentSecretPlayed(Entity entity, string? cardId, int from, int turn, Zone fromZone, int otherId);
		void HandleOpponentHandToDeck(Entity entity, string? cardId, IHsGameState gameState);
		void HandleOpponentPlayToHand(Entity entity, string? cardId, int turn, int id);
		void HandleOpponentPlayToDeck(Entity entity, string? cardId, int turn);
		void HandleOpponentSecretRemove(Entity entity, string? cardId, int turn);
		void HandleOpponentSecretTrigger(Entity entity, string? cardId, int turn, int otherId);
		void HandleOpponentDeckDiscard(Entity entity, string? cardId, int turn);
		void SetOpponentHero(string? cardId);
		void HandleOpponentHeroPower(string cardId, int turn);
		void HandleOpponentGetToDeck(Entity entity, int turn);
		void HandleOpponentCreateInSetAside(Entity entity, int getTurnNumber);

		#endregion OpponentHandlers

		void HandleEntityPredamage(Entity entity, int value);
		void HandleEntityDamage(Entity dealer, Entity target, int value);
		void HandleEntityLostArmor(Entity target, int value);
		void HandleChameleosReveal(string cardId);
		void HandleBeginMulligan();
		void HandlePlayerMulliganDone();
		void HandleCardCopy();
		void HandleProposedAttackerChange(Entity entity);
		void HandlePlayerLibramReduction(int value);
		void HandleOpponentLibramReduction(int value);
		void HandlePlayerHandCostReduction(int value);
		void HandleOpponentHandCostReduction(int value);
		void HandleMercenariesStateChange();
		void HandleBattlegroundsPlayerTechLevel(int id, int value);
		void HandleBattlegroundsPlayerTriples(int id, int value);
		void HandleBattlegroundsHeroReroll(Entity entity, string? oldCardId);
		void HandlePlayerAbyssalCurse(int value);
		void HandleOpponentAbyssalCurse(int value);
		void HandleQuestRewardDatabaseId(int id, int value);
	}
}
