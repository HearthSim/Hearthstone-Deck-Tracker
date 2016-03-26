#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class GameV2 : IGame
	{
		public readonly List<Deck> IgnoredArenaDecks = new List<Deck>();
		private bool _awaitingMainWindowOpen;
		private GameMode _currentGameMode;
		private bool _gameModeDetectionComplete;

		private bool _gameModeDetectionRunning;
		public Deck TempArenaDeck = new Deck();
		private Mode _currentMode;

		public GameV2()
		{
			Player = new Player(true);
			Opponent = new Player(false);

			Entities = new Dictionary<int, Entity>();
			CurrentGameMode = GameMode.None;
			IsInMenu = true;
			PossibleArenaCards = new List<Card>();
			PossibleConstructedCards = new List<Card>();
			OpponentSecrets = new OpponentSecrets(this);
			Reset();
		}

		public List<string> PowerLog { get; } = new List<string>();
		public Deck IgnoreIncorrectDeck { get; set; }
		public GameTime GameTime { get; } = new GameTime();
		public bool IsMinionInPlay => Entities.FirstOrDefault(x => (x.Value.IsInPlay && x.Value.IsMinion)).Value != null;

		public bool IsOpponentMinionInPlay
			=> Entities.FirstOrDefault(x => (x.Value.IsInPlay && x.Value.IsMinion && x.Value.IsControlledBy(Opponent.Id))).Value != null;

		public int OpponentMinionCount => Entities.Count(x => (x.Value.IsInPlay && x.Value.IsMinion && x.Value.IsControlledBy(Opponent.Id)));
		public int PlayerMinionCount => Entities.Count(x => (x.Value.IsInPlay && x.Value.IsMinion && x.Value.IsControlledBy(Player.Id)));

		public Player Player { get; set; }
		public Player Opponent { get; set; }
		public bool IsInMenu { get; set; }
		public bool IsUsingPremade { get; set; }
		public int OpponentSecretCount { get; set; }
		public bool IsRunning { get; set; }
		public Region CurrentRegion { get; set; }
		public GameStats CurrentGameStats { get; set; }
		public OpponentSecrets OpponentSecrets { get; set; }
		public List<Card> DrawnLastGame { get; set; }
		public List<Card> PossibleArenaCards { get; set; }
		public List<Card> PossibleConstructedCards { get; set; }
		public Dictionary<int, Entity> Entities { get; set; }
		public GameMetaData MetaData { get; } = new GameMetaData();
		internal List<Tuple<string, List<string>>> StoredPowerLogs { get; } = new List<Tuple<string, List<string>>>();
		internal Dictionary<int, string> StoredPlayerNames { get; } = new Dictionary<int, string>();
		internal GameStats StoredGameStats { get; set; }

		public Mode CurrentMode
		{
			get { return _currentMode; }
			set
			{
				_currentMode = value;
				Log.Info(value.ToString());
			}
		}

		public Mode PreviousMode { get; set; }

		public bool SavedReplay { get; set; }

		public Entity PlayerEntity => Entities.FirstOrDefault(x => x.Value.IsPlayer).Value;

		public Entity OpponentEntity => Entities.FirstOrDefault(x => x.Value.HasTag(GAME_TAG.PLAYER_ID) && !x.Value.IsPlayer).Value;

		public Entity GameEntity => Entities.FirstOrDefault(x => x.Value?.Name == "GameEntity").Value;

		public bool IsMulliganDone
		{
			get
			{
				var player = Entities.FirstOrDefault(x => x.Value.IsPlayer);
				var opponent = Entities.FirstOrDefault(x => x.Value.HasTag(GAME_TAG.PLAYER_ID) && !x.Value.IsPlayer);
				if(player.Value == null || opponent.Value == null)
					return false;
				return player.Value.GetTag(GAME_TAG.MULLIGAN_STATE) == (int)TAG_MULLIGAN.DONE
					   && opponent.Value.GetTag(GAME_TAG.MULLIGAN_STATE) == (int)TAG_MULLIGAN.DONE;
			}
		}

		public GameMode CurrentGameMode
		{
			get { return _currentGameMode; }
			set
			{
				if(_currentGameMode != value)
				{
					_currentGameMode = value;
					Log.Info("Set CurrentGameMode to " + value);
				}
			}
		}

		public void Reset(bool resetStats = true)
		{
			Log.Info("-------- Reset ---------");

			ReplayMaker.Reset();
			Player.Reset();
			Opponent.Reset();

			Entities.Clear();
			SavedReplay = false;
			OpponentSecretCount = 0;
			OpponentSecrets.ClearSecrets();

			if(!IsInMenu && resetStats)
			{
				if(CurrentGameMode == GameMode.Ranked)
				{
					Log.Info("Resetting gamemode to casual");
					CurrentGameMode = GameMode.Casual;
				}
				CurrentGameStats = new GameStats(GameResult.None, "", "") {PlayerName = "", OpponentName = "", Region = CurrentRegion};
				_gameModeDetectionComplete = false;
			}
			PowerLog.Clear();

			if(Core.Game != null && Core.Overlay != null)
			{
				Helper.UpdatePlayerCards(true);
				Helper.UpdateOpponentCards(true);
			}
		}

		public void SetPremadeDeck(Deck deck)
		{
			foreach(var card in deck.GetSelectedDeckVersion().Cards)
			{
				for(var i = 0; i < card.Count; i++)
					Player.RevealDeckCard(card.Id, -1);
			}
			IsUsingPremade = true;
		}

		public void AddPlayToCurrentGame(PlayType play, int turn, string cardId) => CurrentGameStats?.AddPlay(play, turn, cardId);

		public void ResetArenaCards() => PossibleArenaCards.Clear();

		public void ResetConstructedCards() => PossibleConstructedCards.Clear();

		public async Task GameModeDetection(int timeoutInSeconds = 300)
		{
			if(_gameModeDetectionRunning || _gameModeDetectionComplete)
			{
				while(!_gameModeDetectionComplete)
					await Task.Delay(100);
				return;
			}
			_gameModeDetectionRunning = true;
			var startTime = DateTime.Now;
			var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
			while(CurrentGameMode == GameMode.None && (DateTime.Now - startTime) < timeout)
				await Task.Delay(100);
			if(CurrentGameStats != null && CurrentGameMode != GameMode.None)
			{
				CurrentGameStats.GameMode = CurrentGameMode;
				Log.Info("Detected gamemode, set CurrentGameStats.GameMode=" + CurrentGameMode);
			}
			_gameModeDetectionComplete = true;
			_gameModeDetectionRunning = false;
		}

		public void StoreGameState()
		{
			if(string.IsNullOrEmpty(MetaData.GameId))
				return;
			Log.Info($"Storing PowerLog for gameId={MetaData.GameId}");
			StoredPowerLogs.Add(new Tuple<string, List<string>>(MetaData.GameId, new List<string>(PowerLog)));
			if(Player.Id != -1 && !StoredPlayerNames.ContainsKey(Player.Id))
				StoredPlayerNames.Add(Player.Id, Player.Name);
			if(Opponent.Id != -1 && !StoredPlayerNames.ContainsKey(Opponent.Id))
				StoredPlayerNames.Add(Opponent.Id, Opponent.Name);
			if(StoredGameStats == null)
				StoredGameStats = CurrentGameStats;
		}

		public string GetStoredPlayerName(int id)
		{
			string name;
			StoredPlayerNames.TryGetValue(id, out name);
			return name;
		}

		internal void ResetStoredGameState()
		{
			StoredPowerLogs.Clear();
			StoredPlayerNames.Clear();
			StoredGameStats = null;
		}

		public void NewArenaDeck(string heroId)
		{
			TempArenaDeck = new Deck
			{
				IsArenaDeck = true,
				Class = Database.GetHeroNameFromId(heroId)
			};
			TempArenaDeck.Name = Helper.ParseDeckNameTemplate(Config.Instance.ArenaDeckNameTemplate, TempArenaDeck);
			Log.Info("Created new arena deck: " + TempArenaDeck.Class);
		}

		public void NewArenaCard(string cardId)
		{
			if(TempArenaDeck == null || string.IsNullOrEmpty(cardId))
				return;
			var existingCard = TempArenaDeck.Cards.FirstOrDefault(c => c.Id == cardId);
			if(existingCard != null)
				existingCard.Count++;
			else
				TempArenaDeck.Cards.Add((Card)Database.GetCardFromId(cardId).Clone());
			var numCards = TempArenaDeck.Cards.Sum(c => c.Count);
			Log.Info($"Added new card to arena deck: {cardId} ({numCards}/30)");
			if(numCards == 30)
			{
				Log.Info("Found complete arena deck!");
				if(!Config.Instance.SelectedArenaImportingBehaviour.HasValue)
				{
					Log.Info("...but we are using the old importing method.");
					return;
				}
				var recentArenaDecks = DeckList.Instance.Decks.Where(d => d.IsArenaDeck).OrderByDescending(d => d.LastPlayedNewFirst).Take(15);
				if(recentArenaDecks.Any(d => d.Cards.All(c => TempArenaDeck.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count))))
				{
					Log.Info("...but we already have that one. Discarding.");
					TempArenaDeck.Cards.Clear();
					return;
				}
				if(IgnoredArenaDecks.Any(d => d.Cards.All(c => TempArenaDeck.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count))))
				{
					Log.Info("...but it was already discarded by the user. No automatic action taken.");
					return;
				}
				if(Config.Instance.SelectedArenaImportingBehaviour.Value == ArenaImportingBehaviour.AutoImportSave)
				{
					Log.Info("...auto saving new arena deck.");
					Core.MainWindow.SetNewDeck(TempArenaDeck);
					Core.MainWindow.SaveDeck(false, TempArenaDeck.Version);
					TempArenaDeck.Cards.Clear();
				}
				else if(Config.Instance.SelectedArenaImportingBehaviour.Value == ArenaImportingBehaviour.AutoAsk)
				{
					ShowNewArenaDeckMessageAsync((Deck)TempArenaDeck.Clone());
					TempArenaDeck.Cards.Clear();
				}
			}
		}

		private async void ShowNewArenaDeckMessageAsync(Deck deck)
		{
			if(_awaitingMainWindowOpen)
				return;
			_awaitingMainWindowOpen = true;

			if(Core.MainWindow.WindowState == WindowState.Minimized)
				Core.TrayIcon.ShowMessage("New arena deck detected!");

			while(Core.MainWindow.Visibility != Visibility.Visible || Core.MainWindow.WindowState == WindowState.Minimized)
				await Task.Delay(100);

			var result =
				await
				Core.MainWindow.ShowMessageAsync("New arena deck detected!",
												 "You can change this behaviour to \"auto save&import\" or \"manual\" in [options > tracker > importing]",
												 MessageDialogStyle.AffirmativeAndNegative,
												 new MessageDialogs.Settings {AffirmativeButtonText = "import", NegativeButtonText = "cancel"});

			if(result == MessageDialogResult.Affirmative)
			{
				Log.Info("...saving new arena deck.");
				Core.MainWindow.SetNewDeck(deck);
				Core.MainWindow.ActivateWindow();
			}
			else
				Log.Info("...discarded by user.");
			IgnoredArenaDecks.Add(deck);
			_awaitingMainWindowOpen = false;
		}

		#region Database - Obsolete

		[Obsolete("Use Hearthstone.Database.GetCardFromId", true)]
		public static Card GetCardFromId(string cardId) => Database.GetCardFromId(cardId);

		[Obsolete("Use Hearthstone.Database.GetCardFromName", true)]
		public static Card GetCardFromName(string name, bool localized = false) => Database.GetCardFromName(name, localized);

		[Obsolete("Use Hearthstone.Database.GetActualCards", true)]
		public static List<Card> GetActualCards() => Database.GetActualCards();

		[Obsolete("Use Hearthstone.Database.GetHeroNameFromId", true)]
		public static string GetHeroNameFromId(string id, bool returnIdIfNotFound = true)
			=> Database.GetHeroNameFromId(id, returnIdIfNotFound);

		[Obsolete("Use Hearthstone.Database.IsActualCard", true)]
		public static bool IsActualCard(Card card) => Database.IsActualCard(card);

		#endregion
	}
}