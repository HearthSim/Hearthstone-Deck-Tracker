#region

using System;
using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Replay;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Interfaces
{
	public interface IHsGameState
	{
		bool CurrentEntityHasCardId { get; set; }
		int CurrentEntityId { get; }
		bool GameEnded { get; set; }
		IGameHandler GameHandler { get; set; }
		DateTime LastGameStart { get; set; }
		int LastId { get; set; }
		int MaxId { get; set; }
		bool OpponentUsedHeroPower { get; set; }
		bool PlayerUsedHeroPower { get; set; }
		ReplayKeyPoint ProposedKeyPoint { get; set; }
		bool FoundSpectatorStart { get; set; }
		int JoustReveals { get; set; }
		Dictionary<int, string> KnownCardIds { get; set; }
		int LastCardPlayed { get; set; }
		bool WasInProgress { get; set; }
		bool SetupDone { get; set; }
		int GameTriggerCount { get; set; }
		Zone CurrentEntityZone { get; set; }
		bool DeterminedPlayers { get; }
		void ProposeKeyPoint(KeyPointType type, int id, ActivePlayer player);
		int GetTurnNumber();
		void GameEndKeyPoint(bool victory, int id);
		void Reset();
		void SetCurrentEntity(int id);
		void ResetCurrentEntity();
	}
}