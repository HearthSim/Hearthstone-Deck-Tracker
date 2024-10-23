using System.Collections.Generic;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using HearthWatcher;
using HearthWatcher.Providers;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class Watchers
	{
		static Watchers()
		{
			ArenaWatcher.OnCompleteDeck += (sender, args) => DeckManager.AutoImportArena(Config.Instance.SelectedArenaImportingBehaviour ?? ArenaImportingBehaviour.AutoImportSave, args.Info);
			DungeonRunWatcher.DungeonRunMatchStarted += (newRun, set) => DeckManager.DungeonRunMatchStarted(newRun, set, false);
			DungeonRunWatcher.DungeonInfoChanged += dungeonInfo => DeckManager.UpdateDungeonRunDeck(dungeonInfo, false);
			PVPDungeonRunWatcher.PVPDungeonRunMatchStarted += (newRun, set) => DeckManager.DungeonRunMatchStarted(newRun, set, true);
			PVPDungeonRunWatcher.PVPDungeonInfoChanged += dungeonInfo => DeckManager.UpdateDungeonRunDeck(dungeonInfo, true);
			FriendlyChallengeWatcher.OnFriendlyChallenge += OnFriendlyChallenge;
			ExperienceWatcher.NewExperienceHandler += (sender, args) => Core.Overlay.ExperienceChangedAsync(args.Experience, args.ExperienceNeeded, args.Level, args.LevelChange, args.Animate).Forget();
			QueueWatcher.InQueueChanged += (sender, args) => Core.Game.QueueEvents.Handle(args);
			BaconWatcher.Change += OnBaconChange;
			DeckPickerWatcher.Change += OnDeckPickerChange;
			SceneWatcher.Change += (sender, args) => SceneHandler.OnSceneUpdate((Mode)args.PrevMode, (Mode)args.Mode, args.SceneLoaded, args.Transitioning);
			ChoicesWatcher.Change += (sender, args) => Core.Overlay.SetChoicesVisible(args.CurrentChoice?.IsVisible ?? false);
			BigCardWatcher.Change += OnBigCardChange;
			BattlegroundsTeammateBoardStateWatcher.Change += OnBattlegroundsTeammateBoardStateChange;
			BattlegroundsLeaderboardWatcher.Change += (sender, args) => Core.Overlay.SetHoveredBattlegroundsLeaderboardEntityId(args.HoveredEntityId);
		}

		internal static void Stop()
		{
			ArenaWatcher.Stop();
			PackWatcher.Stop();
			DungeonRunWatcher.Stop();
			PVPDungeonRunWatcher.Stop();
			FriendlyChallengeWatcher.Stop();
			ExperienceWatcher.Stop();
			QueueWatcher.Stop();
			BaconWatcher.Stop();
			DeckPickerWatcher.Stop();
			SceneWatcher.Stop();
			ChoicesWatcher.Stop();
			BigCardWatcher.Stop();
			BattlegroundsTeammateBoardStateWatcher.Stop();
			BattlegroundsLeaderboardWatcher.Stop();
		}

		internal static void OnBaconChange(object sender, HearthWatcher.EventArgs.BaconEventArgs args)
		{
			Core.Overlay.SetBaconState(args.SelectedBattlegroundsGameMode, args.IsShopOpen || args.IsJournalOpen || args.IsPopupShowing || args.IsBlurActive);

			Core.Overlay.SetFriendListOpacityMask(args.IsFriendslistOpen);
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

			Core.Overlay.HoveredCard = state;
		}

		internal static void OnDeckPickerChange(object sender, HearthWatcher.EventArgs.DeckPickerEventArgs args)
		{
			Core.Overlay.SetDeckPickerState(args.SelectedFormatType, args.DecksOnPage, args.IsModalOpen);
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
		public static PackOpeningWatcher PackWatcher { get; } = new(new HearthMirrorPackProvider());
		public static DungeonRunWatcher DungeonRunWatcher { get; } = new(new GameDataProvider());
		public static PVPDungeonRunWatcher PVPDungeonRunWatcher { get; } = new(new GameDataProvider());
		public static FriendlyChallengeWatcher FriendlyChallengeWatcher { get; } = new(new HearthMirrorFriendlyChallengeProvider());
		public static ExperienceWatcher ExperienceWatcher { get; } = new(new HearthMirrorRewardTrackProvider());
		public static QueueWatcher QueueWatcher { get; } = new(new HearthMirrorQueueProvider());
		public static BaconWatcher BaconWatcher { get; } = new(new HearthMirrorBaconProvider());
		public static DeckPickerWatcher DeckPickerWatcher { get; } = new(new HearthMirrorDeckPickerProvider());
		public static SceneWatcher SceneWatcher { get; } = new(new HearthMirrorSceneProvider());
		public static ChoicesWatcher ChoicesWatcher { get; } = new(new HearthMirrorChoicesProvider());

		public static BigCardStateWatcher BigCardWatcher { get; } = new(new HearthMirrorBigCardProvider());
		public static BattlegroundsTeammateBoardStateWatcher BattlegroundsTeammateBoardStateWatcher { get; } = new(new HearthMirrorBattlegroundsTeammateBoardStateProvider());
		public static BattlegroundsLeaderboardWatcher BattlegroundsLeaderboardWatcher { get; } = new(new HearthMirrorBattlegroundsLeaderboardProvider());
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
		public HearthMirror.Objects.Card[]? GetDraftChoices() => Reflection.Client.GetArenaDraftChoicesV2()?.ToArray();
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
		public bool? IsShopOpen => Reflection.Client.IsShopOpen();
		public bool? IsJournalOpen => Reflection.Client.IsJournalOpen();
		public bool? IsPopupShowing => Reflection.Client.IsPopupShowing();
		public bool? IsFriendslistOpen => Reflection.Client.IsFriendsListVisible();
		public bool? IsBlurActive => Reflection.Client.GetIsBlurActive();
		public SelectedBattlegroundsGameMode? SelectedBattlegroundsGameMode => Reflection.Client.GetSelectedBattlegroundsGameMode();
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
}
