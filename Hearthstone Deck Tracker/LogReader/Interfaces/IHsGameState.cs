#region

using System;
using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Replay;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Interfaces
{
	public interface IHsGameState
	{
		int AddToTurn { get; set; }
		bool CurrentEntityHasCardId { get; set; }
		int CurrentEntityId { get; set; }
		long CurrentOffset { get; set; }
		bool DoUpdate { get; set; }
		bool First { get; set; }
		bool GameEnded { get; set; }
		IGameHandler GameHandler { get; set; }
		DateTime LastGameStart { get; set; }
		int LastId { get; set; }
		int MaxId { get; set; }
		bool OpponentUsedHeroPower { get; set; }
		bool PlayerUsedHeroPower { get; set; }
		long PreviousSize { get; set; }
		ReplayKeyPoint ProposedKeyPoint { get; set; }
		dynamic WaitForController { get; set; }
		bool FoundSpectatorStart { get; set; }
		int JoustReveals { get; set; }
		Dictionary<int, string> KnownCardIds { get; set; }
		int LastCardPlayed { get; set; }
		bool WasInProgress { get; set; }
		bool SetupDone { get; set; }
		void ProposeKeyPoint(KeyPointType type, int id, ActivePlayer player);
		int GetTurnNumber();
		void GameEndKeyPoint(bool victory, int id);
		bool PlayersTurn();
		void Reset();
	}
}