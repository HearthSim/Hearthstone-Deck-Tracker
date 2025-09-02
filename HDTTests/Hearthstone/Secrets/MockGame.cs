using System;
using System.Collections.Generic;
using HearthDb.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Hearthstone_Deck_Tracker.Hearthstone.Secrets;
using Hearthstone_Deck_Tracker.Stats;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using Deck = HearthMirror.Objects.Deck;

namespace HDTTests.Hearthstone.Secrets
{
	public class MockGame : IGame
	{
		public MockGame()
		{
			Player = new Player(this, true);
			Opponent = new Player(this, false);
			Entities = new Dictionary<int, Entity>();
		}
		public Player Player { get; set; }
		public Player Opponent { get; set; }
		public Entity GameEntity { get; set; }
		public Entity PlayerEntity { get; set; }
		public Entity OpponentEntity { get; set; }
		public CounterManager CounterManager { get; set; }
		public RelatedCardsManager RelatedCardsManager { get; set; }
		public bool IsMulliganDone { get; set; }
		public bool IsInMenu { get; set; }
		public bool IsUsingPremade { get; set; }
		public bool IsBattlegroundsMatch => IsBattlegroundsSoloMatch || IsBattlegroundsDuosMatch;
		public bool IsBattlegroundsSoloMatch { get; set; }
		public bool IsBattlegroundsDuosMatch { get; set; }
		public bool IsTraditionalHearthstoneMatch { get; set; }
		public bool IsBattlegroundsCombatPhase { get; set; }
		public bool IsRunning { get; set; }
		public Region CurrentRegion { get; set; }
		public GameMode CurrentGameMode { get; set; }
		public GameStats CurrentGameStats { get; set; }
		public Deck CurrentSelectedDeck { get; set; }
		public List<Card> DrawnLastGame { get; set; }
		public Dictionary<int, Entity> Entities { get; set; }
		public bool SavedReplay { get; set; }
		public GameMetaData MetaData { get; }
		public MatchInfo MatchInfo { get; set; }
		public Mode CurrentMode { get; set; }
		public Mode PreviousMode { get; set; }
		public GameTime GameTime { get; set; }
		public void Reset(bool resetStats = true)
		{
			throw new NotImplementedException();
		}

		public void StoreGameState()
		{
			throw new NotImplementedException();
		}

		public string GetStoredPlayerName(int id)
		{
			throw new NotImplementedException();
		}

		public void SnapshotBattlegroundsBoardState()
		{
			throw new NotImplementedException();
		}


		public bool DuosWasPlayerHeroModified { get; set; }
		public bool DuosWasOpponentHeroModified { get; set;  }

		public void DuosSetHeroModified(bool isPlayer) => throw new NotImplementedException();

		public void DuosResetHeroTracking()
		{
			DuosWasPlayerHeroModified = false;
			DuosWasOpponentHeroModified = false;
		}

		public BoardSnapshot GetBattlegroundsBoardStateFor(int entityId)
		{
			throw new NotImplementedException();
		}

		public int GetTurnNumber()
		{
			throw new NotImplementedException();
		}

		public SecretsManager SecretsManager { get; set; }
		public int OpponentMinionCount { get; set; }
		public int OpponentBoardCount { get; set; }
		public int OpponentHandCount { get; set; }
		public bool IsMinionInPlay { get; set; }
		public int PlayerMinionCount { get; set; }
		public int PlayerBoardCount { get; set; }
		public GameType CurrentGameType { get; set; }
		public FormatType CurrentFormatType { get; set; }
		public Format? CurrentFormat { get; set; }
		public int ProposedAttacker { get; set; }
		public int ProposedDefender { get; set; }
		public bool? IsDungeonMatch { get; set; }
		public bool PlayerChallengeable { get; }
		public bool SetupDone { get; set; }

		public int OpponentSecretCount { get; set; }
	}
}
