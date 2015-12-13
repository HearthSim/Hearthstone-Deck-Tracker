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
		Player Player { get; set; }
		Player Opponent { get; set; }
		Entity PlayerEntity { get; }
		Entity OpponentEntity { get; }
        bool IsMulliganDone { get; }
        bool NoMatchingDeck { get; set; }
        bool IsInMenu { get; set; }
        bool IsUsingPremade { get; set; }
        int OpponentSecretCount { get; set; }
        bool IsRunning { get; set; }
        Region CurrentRegion { get; set; }
        GameMode CurrentGameMode { get; set; }
        GameStats CurrentGameStats { get; set; }
        OpponentSecrets OpponentSecrets { get; set; }
        List<Card> DrawnLastGame { get; set; }
        List<Card> PossibleArenaCards { get; set; }
        List<Card> PossibleConstructedCards { get; set; }
        Dictionary<int, Entity> Entities { get; set; }
        bool SavedReplay { get; set; }
        void Reset(bool resetStats = true);
        void SetPremadeDeck(Deck deck);
        void AddPlayToCurrentGame(PlayType play, int turn, string cardId);
        void ResetArenaCards();
        void ResetConstructedCards();
        void NewArenaDeck(string heroId);
        void NewArenaCard(string cardId);
	    Task GameModeDetection(int timeout);
    }
}