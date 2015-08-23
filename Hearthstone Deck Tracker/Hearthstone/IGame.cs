using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
    public interface IGame
    {
        bool IsMulliganDone { get; }
        int PlayerDeckSize { get; set; }
        bool NoMatchingDeck { get; set; }
        bool HighlightCardsInHand { get; set; }
        bool HighlightDiscarded { get; set; }
        ObservableCollection<Card> OpponentCards { get; set; }
        int OpponentHandCount { get; set; }
        int OpponentFatigueCount { get; set; }
        bool IsInMenu { get; set; }
        bool IsUsingPremade { get; set; }
        int OpponentDeckCount { get; set; }
        bool OpponentHasCoin { get; set; }
        int OpponentSecretCount { get; set; }
        bool IsRunning { get; set; }
        Region CurrentRegion { get; set; }
        GameMode CurrentGameMode { get; set; }
        GameStats CurrentGameStats { get; set; }
        ObservableCollection<Card> PlayerDeck { get; set; }
        ObservableCollection<Card> PlayerDrawn { get; set; }
        int PlayerHandCount { get; set; }
        int PlayerFatigueCount { get; set; }
        string PlayingAgainst { get; set; }
        string PlayingAs { get; set; }
        string PlayerName { get; set; }
        string OpponentName { get; set; }
        List<string> SetAsideCards { get; set; }
        List<KeyValuePair<string, int>> OpponentReturnedToDeck { get; set; }
        OpponentSecrets OpponentSecrets { get; set; }
        List<Card> DrawnLastGame { get; set; }
        int[] OpponentHandAge { get; }
        CardMark[] OpponentHandMarks { get; }
        Card[] OpponentStolenCardsInformation { get; }
        List<Card> PossibleArenaCards { get; set; }
        List<Card> PossibleConstructedCards { get; set; }
        int? SecondToLastUsedId { get; set; }
        Dictionary<int, Entity> Entities { get; set; }
        int PlayerId { get; set; }
        int OpponentId { get; set; }
        bool SavedReplay { get; set; }
        void Reset(bool resetStats = true);
        void SetPremadeDeck(Deck deck);
        void AddPlayToCurrentGame(PlayType play, int turn, string cardId);
        void ResetArenaCards();
        void ResetConstructedCards();
        Task<bool> PlayerDraw(string cardId);
        void PlayerGet(string cardId, bool fromPlay, int turn);
        void PlayerPlayed(string cardId);
        void PlayerMulligan(string cardId);
        void PlayerHandDiscard(string cardId);
        bool PlayerDeckDiscard(string cardId);
        void PlayerPlayToDeck(string cardId);
        void PlayerGetToDeck(string cardId, int turn);
        void OpponentGetToDeck(int turn);
        void OpponentDraw(int turn);
        void OpponentJoustReveal(string cardId);
        void OpponentPlay(string id, int from, int turn);
        void OpponentMulligan(int pos);
        void OpponentBackToHand(string cardId, int turn, int id);
        void OpponentPlayToDeck(string cardId, int turn);
        void OpponentDeckDiscard(string cardId);
        void OpponentSecretTriggered(string cardId);
        void OpponentGet(int turn, int id);
        void NewArenaDeck(string heroId);
        void NewArenaCard(string cardId);
    }
}