using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Controls.Overlay.Arena;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.MinionPinning;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Utility.Arena;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using HearthWatcher;
using HearthWatcher.Providers;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class Watchers
	{
		static Watchers()
		{
			ArenaWatcher.OnChoicesChanged += OnChoiceChanged;
			ArenaWatcher.OnCardPicked += OnCardPicked;
			ArenaWatcher.OnCompleteDeck += OnDeckCompleted;
			ArenaWatcher.OnRedraftChoicesChanged += OnRedraftChoiceChanged;
			ArenaWatcher.OnRedraftCardPicked += OnRedraftCardPicked;
			DungeonRunWatcher.DungeonRunMatchStarted += (newRun, set) => DeckManager.DungeonRunMatchStarted(newRun, set, false);
			DungeonRunWatcher.DungeonInfoChanged += dungeonInfo => DeckManager.UpdateDungeonRunDeck(dungeonInfo, false);
			PVPDungeonRunWatcher.PVPDungeonRunMatchStarted += (newRun, set) => DeckManager.DungeonRunMatchStarted(newRun, set, true);
			PVPDungeonRunWatcher.PVPDungeonInfoChanged += dungeonInfo => DeckManager.UpdateDungeonRunDeck(dungeonInfo, true);
			FriendlyChallengeWatcher.OnFriendlyChallenge += OnFriendlyChallenge;
			ExperienceWatcher.NewExperienceHandler += (sender, args) => Core.Overlay.ExperienceChangedAsync(args.Experience, args.ExperienceNeeded, args.Level, args.LevelChange, args.Animate).Forget();
			QueueWatcher.InQueueChanged += (sender, args) => Core.Game.QueueEvents.Handle(args);
			BaconWatcher.Change += OnBaconChange;
			UiWatcher.Change += OnUiChange;
			DeckPickerWatcher.Change += OnDeckPickerChange;
			SceneWatcher.Change += (sender, args) => SceneHandler.OnSceneUpdate((Mode)args.PrevMode, (Mode)args.Mode, args.SceneLoaded, args.Transitioning);
			ChoicesWatcher.Change += (sender, args) => Core.Overlay.SetChoicesVisible(args.CurrentChoice?.IsVisible ?? false, args.CurrentChoice?.Cards);
			DiscoverStateWatcher.Change += OnDiscoverStateChange;
			BigCardWatcher.Change += OnBigCardChange;
			OpponentBoardStateWatcher.Change += OnOpponentBoardStateChange;
			BattlegroundsTeammateBoardStateWatcher.Change += OnBattlegroundsTeammateBoardStateChange;
			BattlegroundsLeaderboardWatcher.Change += (sender, args) => Core.Overlay.SetHoveredBattlegroundsLeaderboardEntityId(args.HoveredEntityId);
			MulliganTooltipWatcher.Change += OnMulliganTooltipChange;
		}

		internal static void Stop()
		{
			ArenaWatcher.Stop();
			ArenaStateWatcher.Stop();
			PackWatcher.Stop();
			DungeonRunWatcher.Stop();
			PVPDungeonRunWatcher.Stop();
			FriendlyChallengeWatcher.Stop();
			ExperienceWatcher.Stop();
			QueueWatcher.Stop();
			BaconWatcher.Stop();
			UiWatcher.Stop();
			DeckPickerWatcher.Stop();
			SceneWatcher.Stop();
			ChoicesWatcher.Stop();
			BigCardWatcher.Stop();
			DiscoverStateWatcher.Stop();
			OpponentBoardStateWatcher.Stop();
			BattlegroundsTeammateBoardStateWatcher.Stop();
			BattlegroundsLeaderboardWatcher.Stop();
			MulliganTooltipWatcher.Stop();
		}

		private static readonly Dictionary<long, Dictionary<int, (string[] choices, string[][]? packages, string pickStartTime)>> _currentArenaDraftInfo = new();

		internal static void OnChoiceChanged(object sender, HearthWatcher.EventArgs.ChoicesChangedEventArgs args)
		{
			var newChoices = args.Choices.Select(c => c.Id).ToArray();
			var packages = args.Packages?.Select(p => p.Select(c => c.Id).ToArray()).ToArray();
			var pickStartTime = DateTime.Now.ToString("o");

			var deckId = args.Deck.Id;

			if (!_currentArenaDraftInfo.TryGetValue(deckId, out var draftInfo))
				_currentArenaDraftInfo[deckId] = draftInfo = new Dictionary<int, (string[] choices, string[][]? packages, string pickStartTime)>();

			draftInfo[args.Slot] = (newChoices, packages, pickStartTime);
		}

		internal static void OnCardPicked(object sender, HearthWatcher.EventArgs.CardPickedEventArgs args)
		{

			var packageSize = args.PickedPackage?.Count ?? 0;

			if (!TryGetDraftInfo(args.Deck.Id, args.Slot, packageSize, out var info))
				return;

			var currentPick = args.Picked.Id;

			var pickedCards = args.Deck.Cards
				.WhereNotNull()
				.SelectMany(c => Enumerable.Repeat(c.Id, c.Id == currentPick ? Math.Max(0, c.Count - 1) : c.Count))
				.ToArray();

			var packages = StructurePackages(info.choices, info.packages);

			var pickTime = DateTime.Now.ToString("o");
			ArenaLastDrafts.Instance.AddPick(
				info.pickStartTime,
				pickTime,
				args.Picked.Id,
				info.choices,
				args.Slot,
				pickedCards,
				args.Deck.Id,
				args.IsUnderground,
				args.PickedPackage?.Select(c => c.Id).ToArray() ?? null,
				packages,
				isOverlayEnabled: Config.Instance.EnableArenasmithOverlay && Config.Instance.ShowArenasmithScore,
				overlayVisible: Core.Overlay.ArenaPickHelperViewModel.IsOverlayVisible,
				isArenasmithAvailable: Core.Overlay.ArenaPickHelperViewModel.IsArenasmithAvailable,
				isTrialsActivated: Core.Overlay.ArenaPickHelperViewModel.IsTrialsActivated,
				arenasmithScores: Core.Overlay.ArenaPickHelperViewModel.ArenasmithScores
			);

			const int lastPickSlot = 30;
			if(args.Slot == lastPickSlot)
			{
				Core.Overlay.HideArenaPickHelper();
			}
		}

		internal static void OnRedraftChoiceChanged(object sender, HearthWatcher.EventArgs.RedraftChoicesChangedEventArgs args)
		{

			var newChoices = args.Choices.Select(c => c.Id).ToArray();
			var pickStartTime = DateTime.Now.ToString("o");

			var deckId = args.RedraftDeck.Id;

			if (!_currentArenaDraftInfo.TryGetValue(deckId, out var draftInfo))
				_currentArenaDraftInfo[deckId] = draftInfo = new Dictionary<int, (string[] choices, string[][]? packages, string pickStartTime)>();

			draftInfo[args.Slot] = (newChoices, null, pickStartTime);

		}

		internal static void OnRedraftCardPicked(object sender, HearthWatcher.EventArgs.RedraftCardPickedEventArgs args)
		{

			if (!TryGetDraftInfo(args.RedraftDeck.Id, args.Slot, 0, out var info))
				return;

			var currentPick = args.Picked.Id;

			var originalDeck = args.Deck.Cards
				.WhereNotNull()
				.SelectMany(c => Enumerable.Repeat(c.Id, c.Count))
				.ToArray();

			var redraftDeck = args.RedraftDeck.Cards
				.WhereNotNull()
				.SelectMany(c => Enumerable.Repeat(c.Id, c.Id == currentPick ? Math.Max(0, c.Count - 1) : c.Count))
				.ToArray();

			var pickTime = DateTime.Now.ToString("o");
			ArenaLastDrafts.Instance.AddRedraftPick(
				info.pickStartTime,
				pickTime,
				args.Picked.Id,
				info.choices,
				args.Slot,
				originalDeck,
				redraftDeck,
				args.Deck.Id,
				args.RedraftDeck.Id,
				args.Losses,
				args.IsUnderground,
				isOverlayEnabled: Config.Instance.EnableArenasmithOverlay && Config.Instance.ShowArenasmithScore,
				overlayVisible: Core.Overlay.ArenaPickHelperViewModel.IsOverlayVisible,
				isArenasmithAvailable: Core.Overlay.ArenaPickHelperViewModel.IsArenasmithAvailable,
				isTrialsActivated: Core.Overlay.ArenaPickHelperViewModel.IsTrialsActivated,
				arenasmithScores: Core.Overlay.ArenaPickHelperViewModel.ArenasmithScores
			);

		}

		private static bool TryGetDraftInfo(long deckId, int slot, int packageSize,
			out (string[] choices, string[][]? packages, string pickStartTime) info)
		{
			info = default;

			if(!_currentArenaDraftInfo.TryGetValue(deckId, out var draftInfo))
				return false;

			// try original slot
			if (draftInfo.TryGetValue(slot, out info) &&
			    info.choices is { Length: > 0 } &&
			    !string.IsNullOrEmpty(info.pickStartTime))
			{
				return true;
			}

			// try fallback to (slot - package size)
			if (packageSize > 0)
			{
				var fallbackSlot = slot - packageSize;
				if (draftInfo.TryGetValue(fallbackSlot, out info) &&
				    info.choices is { Length: > 0 } &&
				    !string.IsNullOrEmpty(info.pickStartTime))
				{
					return true;
				}
			}


			return false;
		}

		private static Dictionary<string, string[]>? StructurePackages(string[] choices, string[][]? packages)
		{
			if(packages == null || packages.Length == 0) return null;

			var structuredPackages = new Dictionary<string, string[]>();
			for(var i = 0; i < choices.Length; i++)
			{
				if(i >= packages.Length) break;

				structuredPackages.Add(choices[i], packages[i]);
			}

			return structuredPackages;
		}

		internal static void OnDeckCompleted(object sends, HearthWatcher.EventArgs.CompleteDeckEventArgs args)
		{
			DeckManager.AutoImportArena(args.Info);
			_currentArenaDraftInfo.Remove(args.Info.Deck.Id);
		}

		internal static void OnBaconChange(object sender, HearthWatcher.EventArgs.BaconEventArgs args)
		{
			Core.Overlay.SetBaconState(args.SelectedBattlegroundsGameMode);
		}

		internal static void OnUiChange(object sender, HearthWatcher.EventArgs.UIEventArgs args)
		{
			var isCriticalUiOpen = args.IsShopOpen || args.IsJournalOpen || args.IsPopupShowing || args.IsBlurActive;
			Core.Overlay.Tier7PreLobbyViewModel.IsGameCriticalUiOpen = isCriticalUiOpen;
			Core.Overlay.ArenaPreDraftViewModel.IsGameCriticalUiOpen = isCriticalUiOpen;
			Core.Overlay.SetFriendListOpacityMask(args.IsFriendsListVisible);
		}

		internal static void OnBigCardChange(object sender, HearthWatcher.EventArgs.BigCardArgs args)
		{
			var state = new BigCardState
				{
					CardId = args.CardId,
					EnchantmentHeights = args.EnchantmentHeights,
					TooltipHeights = args.TooltipHeights,
					ZonePosition = args.ZonePosition,
					ZoneSize = args.ZoneSize,
					Side = args.Side,
					IsHand = args.IsHand
				}
			;

			Core.Overlay.SetCardOpacityMask(state);
			Core.Overlay.SetRelatedCardsTrigger(state);

			if(Core.Game.IsTraditionalHearthstoneMatch)
			{
				var isFriendlyCard = state.Side == (int)Side.FRIENDLY;
				Core.Overlay.HighlightPlayerDeckCards(isFriendlyCard ? state.CardId : null);
				if(Core.Windows.PlayerWindow.IsVisible)
				{
					Core.Windows.PlayerWindow.HighlightPlayerDeckCards(isFriendlyCard ? state.CardId : null);
				}
			}

			Core.Overlay.SetAnomalyGuidesTrigger(state.CardId);
		}

		internal static void OnDiscoverStateChange(object sender, HearthWatcher.EventArgs.DiscoverStateArgs args)
		{
			var state = new DiscoverState
			{
				CardId = args.CardId,
				ZoneSize = args.ZoneSize,
				ZonePosition = args.ZonePosition,
				EntityId = args.EntityId
			};

			Core.Overlay.SetTrinketGuidesTrigger(state.ZoneSize, state.ZonePosition, state.CardId);
			Core.Overlay.SetRelatedCardsTrigger(state);
			Core.Overlay.SetQuestGuidesTrigger(state);

			if(Core.Game.IsTraditionalHearthstoneMatch)
			{
				Core.Overlay.HighlightPlayerDeckCards(args.CardId);
				if(Core.Windows.PlayerWindow.IsVisible)
				{
					Core.Windows.PlayerWindow.HighlightPlayerDeckCards(state.CardId);
				}
			}
		}

		internal static void OnMulliganTooltipChange(object sender, HearthWatcher.EventArgs.MulliganTooltipArgs args)
		{
			var buddiesEnabled = Core.Game.GameEntity?.GetTag(GameTag.BACON_BUDDY_ENABLED) > 0;
			Core.Overlay.SetHeroPickingTooltipMask(
				args.ZoneSize, args.ZonePosition, args.IsTooltipOnRight, args.TooltipCards.Length, buddiesEnabled
			);

			Core.Overlay.SetHeroGuidesTrigger(args.ZoneSize, args.ZonePosition, args.IsTooltipOnRight, args.TooltipCards, buddiesEnabled);
		}

		internal static void OnDeckPickerChange(object sender, HearthWatcher.EventArgs.DeckPickerEventArgs args)
		{
			Core.Overlay.SetDeckPickerState(args.SelectedFormatType, args.DecksOnPage, args.IsModalOpen);
		}

		internal static void OnOpponentBoardStateChange (object sender, HearthWatcher.EventArgs.OpponentBoardArgs args)
		{
			var boardCards = args.BoardCards;
			var mousedOverSlot = args.MousedOverSlot;

			if(Core.Game.IsBattlegroundsMatch)
				Core.Overlay.BattlegroundsMinionPinningViewModel.OnShopChange(boardCards, mousedOverSlot);
		}

		internal static void OnFriendlyChallenge(object sender, HearthWatcher.EventArgs.FriendlyChallengeEventArgs args)
		{
			if(args.DialogVisible)
			{
				switch(Config.Instance.ChallengeAction)
				{
					case HsActionType.Flash:
						User32.FlashHs();
						break;
					case HsActionType.Popup:
						User32.BringHsToForeground();
						break;
				}
			}
		}

		internal static void OnBattlegroundsTeammateBoardStateChange(object sender, HearthWatcher.EventArgs.BattlegroundsTeammateBoardStateArgs args)
		{
			Core.Overlay.BattlegroundsHeroPickingViewModel.IsViewingTeammate = args.IsViewingTeammate;
			Core.Overlay.IsViewingBGsTeammate = args.IsViewingTeammate;

			if(args is { IsViewingTeammate: false, Entities.Count: 0, MulliganHeroes.Count: 0 })
			{
				Core.Game.BattlegroundsDuosBoardState = null;
				return;
			}

			var state = new BattlegroundsDuosBoardState(args.IsViewingTeammate, args.MulliganHeroes, args.Entities);
			Core.Game.BattlegroundsDuosBoardState = state;
		}

		public static ArenaWatcher ArenaWatcher { get; } = new(new HearthMirrorArenaProvider());
		public static ArenaStateWatcher ArenaStateWatcher{ get; } = new(new HearthMirrorArenaStateProvider());
		public static PackOpeningWatcher PackWatcher { get; } = new(new HearthMirrorPackProvider());
		public static DungeonRunWatcher DungeonRunWatcher { get; } = new(new GameDataProvider());
		public static PVPDungeonRunWatcher PVPDungeonRunWatcher { get; } = new(new GameDataProvider());
		public static FriendlyChallengeWatcher FriendlyChallengeWatcher { get; } = new(new HearthMirrorFriendlyChallengeProvider());
		public static ExperienceWatcher ExperienceWatcher { get; } = new(new HearthMirrorRewardTrackProvider());
		public static QueueWatcher QueueWatcher { get; } = new(new HearthMirrorQueueProvider());
		public static BaconWatcher BaconWatcher { get; } = new(new HearthMirrorBaconProvider());
		public static UiWatcher UiWatcher { get; } = new(new HearthMirrorUiProvider());
		public static DeckPickerWatcher DeckPickerWatcher { get; } = new(new HearthMirrorDeckPickerProvider());
		public static SceneWatcher SceneWatcher { get; } = new(new HearthMirrorSceneProvider());
		public static ChoicesWatcher ChoicesWatcher { get; } = new(new HearthMirrorChoicesProvider());
		public static BigCardStateWatcher BigCardWatcher { get; } = new(new HearthMirrorBigCardProvider());
		public static OpponentBoardStateWatcher OpponentBoardStateWatcher { get; } =
			new(new HearthMirrorOpponentBoardStateProvider());
		public static DiscoverStateWatcher DiscoverStateWatcher { get; } = new(new HearthMirrorDiscoverStateProvider());
		public static BattlegroundsTeammateBoardStateWatcher BattlegroundsTeammateBoardStateWatcher { get; } = new(new HearthMirrorBattlegroundsTeammateBoardStateProvider());
		public static BattlegroundsLeaderboardWatcher BattlegroundsLeaderboardWatcher { get; } = new(new HearthMirrorBattlegroundsLeaderboardProvider());
		public static MulliganTooltipWatcher MulliganTooltipWatcher { get; } = new(new HearthMirrorMulliganTooltipProvider());
	}

	public class GameDataProvider : IGameDataProvider
	{
		public bool InAiMatch => Core.Game.CurrentMode == Mode.GAMEPLAY && Core.Game.MatchInfo?.GameType == (int)GameType.GT_VS_AI;
		public bool InAdventureScreen => Core.Game.CurrentMode == Mode.ADVENTURE;
		public bool InPVPDungeonRunScreen
		{
			get
			{
				return Core.Game.CurrentMode == Mode.PVP_DUNGEON_RUN;
			}
		}

		public bool InPVPDungeonRunMatch
		{
			get
			{
				return Core.Game.CurrentMode == Mode.GAMEPLAY && Core.Game.PreviousMode == Mode.PVP_DUNGEON_RUN;
			}
		}
		public string? OpponentHeroId => Core.Game.Opponent.Hero?.CardId;
		public int OpponentHeroHealth => Core.Game.Opponent.Hero?.GetTag(GameTag.HEALTH) ?? 0;
	}

	public class HearthMirrorPackProvider : IPackProvider
	{
		public List<HearthMirror.Objects.Card> GetCards() => Reflection.Client.GetPackCards();
		public int GetPackId() => Reflection.Client.GetLastOpenedBoosterId();
	}

	public class HearthMirrorArenaProvider : IArenaProvider
	{
		public ArenaInfo? GetArenaInfo() => DeckImporter.FromArena(false);
		public DraftChoices? GetDraftChoices() => Reflection.Client.GetArenaDraftChoicesV3();
	}

	public class HearthMirrorArenaStateProvider : IArenaStateProvider
	{
		public ArenaState? GetState(int? deckListVersion, int? redraftDeckListVersion, ArenaState.ScryCache? cache)
			=> Reflection .Client.GetArenaState (deckListVersion, redraftDeckListVersion, cache);

		public bool IsBlurActive => Reflection.Client.GetIsBlurActive();
	}

	public class HearthMirrorFriendlyChallengeProvider : IFriendlyChallengeProvider
	{
		public bool DialogVisible => Reflection.Client.IsFriendlyChallengeDialogVisible();
	}

	public class HearthMirrorRewardTrackProvider : IExperienceProvider
	{
		public RewardTrackData GetRewardTrackData() => Reflection.Client.GetRewardTrackData();
	}

	public class HearthMirrorQueueProvider : IQueueProvider
	{
		public FindGameState? FindGameState => Reflection.Client.GetFindGameState();
	}

	public class HearthMirrorBaconProvider : IBaconProvider
	{
		public SelectedBattlegroundsGameMode? SelectedBattlegroundsGameMode => Reflection.Client.GetSelectedBattlegroundsGameMode();
	}

	public class HearthMirrorUiProvider : IUiProvider
	{
		public bool? IsShopOpen => Reflection.Client.IsShopOpen();
		public bool? IsJournalOpen => Reflection.Client.IsJournalOpen();
		public bool? IsPopupShowing => Reflection.Client.IsPopupShowing();
		public bool? IsFriendsListVisible => Reflection.Client.IsFriendsListVisible();
		public bool? IsBlurActive => Reflection.Client.GetIsBlurActive();
	}

	public class HearthMirrorDeckPickerProvider : IDeckPickerProvider
	{
		public List<CollectionDeckBoxVisual?>? DecksOnPage => Reflection.Client.GetDeckPickerDecksOnPage();
		public DeckPickerState? DeckPickerState => Reflection.Client.GetDeckPickerState();
		public bool IsBlurActive => Reflection.Client.GetIsBlurActive();
	}

	public class HearthMirrorSceneProvider : ISceneProvider
	{
		public SceneMgrState? State => Reflection.Client.GetSceneMgrState();
	}

	public class HearthMirrorBigCardProvider : IBigCardProvider
	{
		public BigCardState? State => Reflection.Client.GetBigCardState();
	}

	public class HearthMirrorDiscoverStateProvider : IDiscoverStateProvider
	{
		public DiscoverState? State => Reflection.Client.GetDiscoverState();
	}

	public class HearthMirrorBattlegroundsTeammateBoardStateProvider : IBattlegroundsTeammateBoardStateProvider
	{
		public BattlegroundsTeammateBoardState? BattlegroundsTeammateBoardState =>
			Reflection.Client.GetBattlegroundsTeammateBoardState();
	}

	public class HearthMirrorBattlegroundsLeaderboardProvider : IBattlegroundsLeaderboardProvider
	{
		public int? BattlegroundsLeaderboardHoveredEntityId =>
			Reflection.Client.GetBattlegroundsLeaderboardHoveredEntityId();
	}

	public class HearthMirrorChoicesProvider : IChoicesProvider
	{
		public CardChoices? CurrentChoice => Reflection.Client.GetCardChoices();
	}

	public class HearthMirrorMulliganTooltipProvider : IMulliganTooltipProvider
	{
		public MulliganTooltipState? State => Reflection.Client.GetMulliganTooltipState();
	}

	public class HearthMirrorOpponentBoardStateProvider : IOpponentBoardProvider
	{
		public OpponentBoardState? OpponentBoardState => Reflection.Client.GetOpponentBoardState();
	}
}
