using System;
using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Replay;

namespace Hearthstone_Deck_Tracker.LogReader.Interfaces
{
    public interface IHsGameState
    {
        int AddToTurn { get; set; }
        bool AwaitingRankedDetection { get; set; }
        bool CurrentEntityHasCardId { get; set; }
        int CurrentEntityId { get; set; }
        long CurrentOffset { get; set; }
        bool DoUpdate { get; set; }
        bool First { get; set; }
        bool FoundRanked { get; set; }
        bool GameEnded { get; set; }
        IGameHandler GameHandler { get; set; }
        bool GameLoaded { get; set; }
        DateTime LastAssetUnload { get; set; }
        long LastGameEnd { get; set; }
        DateTime LastGameStart { get; set; }
        int LastId { get; set; }
        bool OpponentUsedHeroPower { get; set; }
        bool PlayerUsedHeroPower { get; set; }
        long PreviousSize { get; set; }
        ReplayKeyPoint ProposedKeyPoint { get; set; }
        dynamic WaitForController { get; set; }
        bool WaitingForFirstAssetUnload { get; set; }
        bool FoundSpectatorStart { get; set; }
	    int JoustReveals { get; set; }
	    Dictionary<int, string> KnownCardIds { get; set; }
	    void ProposeKeyPoint(KeyPointType type, int id, ActivePlayer player);
        void GameEnd();
        int GetTurnNumber();
        void GameEndKeyPoint(bool victory, int id);
    }
}