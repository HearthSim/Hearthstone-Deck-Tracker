using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Replay;

namespace Hearthstone_Deck_Tracker.LogReader
{
    public class HsGameState
    {
        public int AddToTurn { get; set; }
        public bool AwaitingRankedDetection { get; set; }
        public bool CurrentEntityHasCardId { get; set; }
        public int CurrentEntityId { get; set; }
        public long CurrentOffset { get; set; }
        public bool DoUpdate { get; set; }
        public bool First { get; set; }
        public bool FoundRanked { get; set; }
        public bool GameEnded { get; set; }
        public IGameHandler GameHandler { get; set; }
        public bool GameLoaded { get; set; }
        public DateTime LastAssetUnload { get; set; }
        public long LastGameEnd { get; set; }
        public DateTime LastGameStart { get; set; }
        public int LastId { get; set; }
        public bool OpponentUsedHeroPower { get; set; }
        public bool PlayerUsedHeroPower { get; set; }
        public long PreviousSize { get; set; }
        public ReplayKeyPoint ProposedKeyPoint { get; set; }
        public dynamic WaitForController { get; set; }
        public bool WaitingForFirstAssetUnload { get; set; }
        public bool FoundSpectatorStart { get; set; }
        public bool NextUpdatedEntityIsJoust { get; set; }


        public void ProposeKeyPoint(KeyPointType type, int id, ActivePlayer player)
        {
            if (ProposedKeyPoint != null)
                ReplayMaker.Generate(ProposedKeyPoint.Type, ProposedKeyPoint.Id, ProposedKeyPoint.Player);
            ProposedKeyPoint = new ReplayKeyPoint(null, type, id, player);
        }
        
        public void GameEnd()
        {
            LastGameEnd = CurrentOffset;
        }
        
        public int GetTurnNumber()
        {
            if (!Game.IsMulliganDone)
                return 0;
            if (AddToTurn == -1)
            {
                var firstPlayer = Game.Entities.FirstOrDefault(e => e.Value.HasTag(GAME_TAG.FIRST_PLAYER));
                if (firstPlayer.Value != null)
                    AddToTurn = firstPlayer.Value.GetTag(GAME_TAG.CONTROLLER) == Game.PlayerId ? 0 : 1;
            }
            Entity entity;
            if (Game.Entities.TryGetValue(1, out entity))
                return (entity.Tags[GAME_TAG.TURN] + (AddToTurn == -1 ? 0 : AddToTurn)) / 2;
            return 0;
        }
        
        public void GameEndKeyPoint(bool victory, int id)
        {
            if (ProposedKeyPoint != null)
            {
                ReplayMaker.Generate(ProposedKeyPoint.Type, ProposedKeyPoint.Id, ProposedKeyPoint.Player);
                ProposedKeyPoint = null;
            }
            ReplayMaker.Generate(victory ? KeyPointType.Victory : KeyPointType.Defeat, id, ActivePlayer.Player);
        }
    }
}